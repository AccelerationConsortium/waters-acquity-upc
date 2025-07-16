"""
Data Analysis Utilities for Waters Acquity UPC

This module provides utilities for analyzing chromatography data
collected from the Waters Acquity system.
"""

import numpy as np
import pandas as pd
from typing import Tuple, List, Optional
import matplotlib.pyplot as plt
from scipy.signal import find_peaks, savgol_filter
from datetime import datetime


class ChromatographyAnalyzer:
    """
    Analyzer for chromatography data from Waters Acquity UPC.
    """
    
    def __init__(self, data: pd.DataFrame):
        """
        Initialize the analyzer with chromatography data.
        
        Args:
            data: DataFrame containing chromatography data with columns:
                  'time', 'signal', 'pressure', 'temperature'
        """
        self.data = data.copy()
        self.peaks = None
        self.baseline = None
        
    def smooth_signal(self, window_length: int = 11, polyorder: int = 3) -> pd.Series:
        """
        Apply Savitzky-Golay smoothing to the signal.
        
        Args:
            window_length: Length of the smoothing window (must be odd)
            polyorder: Order of the polynomial used for smoothing
            
        Returns:
            Smoothed signal as pandas Series
        """
        if window_length % 2 == 0:
            window_length += 1  # Ensure odd window length
            
        smoothed = savgol_filter(self.data['signal'], window_length, polyorder)
        self.data['smoothed_signal'] = smoothed
        return pd.Series(smoothed, index=self.data.index)
    
    def find_peaks(self, height: float = None, distance: int = None, 
                   prominence: float = None) -> Tuple[np.ndarray, dict]:
        """
        Find peaks in the chromatogram.
        
        Args:
            height: Minimum peak height
            distance: Minimum distance between peaks (in data points)
            prominence: Minimum peak prominence
            
        Returns:
            Tuple of (peak_indices, peak_properties)
        """
        signal = self.data.get('smoothed_signal', self.data['signal'])
        
        # Set default parameters if not provided
        if height is None:
            height = np.mean(signal) + 2 * np.std(signal)
        if distance is None:
            distance = len(signal) // 20  # Default to 5% of data length
        if prominence is None:
            prominence = np.std(signal)
        
        peaks, properties = find_peaks(
            signal, 
            height=height, 
            distance=distance, 
            prominence=prominence
        )
        
        self.peaks = peaks
        return peaks, properties
    
    def calculate_peak_areas(self, peaks: np.ndarray = None) -> List[float]:
        """
        Calculate peak areas using trapezoidal integration.
        
        Args:
            peaks: Peak indices (if None, uses previously found peaks)
            
        Returns:
            List of peak areas
        """
        if peaks is None:
            if self.peaks is None:
                raise ValueError("No peaks found. Run find_peaks() first.")
            peaks = self.peaks
        
        signal = self.data.get('smoothed_signal', self.data['signal'])
        time = self.data['time']
        areas = []
        
        for i, peak_idx in enumerate(peaks):
            # Determine peak boundaries (simple approach)
            start_idx = peaks[i-1] + (peak_idx - peaks[i-1]) // 2 if i > 0 else 0
            end_idx = peak_idx + (peaks[i+1] - peak_idx) // 2 if i < len(peaks) - 1 else len(signal) - 1
            
            # Calculate area using trapezoidal rule
            peak_time = time.iloc[start_idx:end_idx+1]
            peak_signal = signal.iloc[start_idx:end_idx+1]
            area = np.trapz(peak_signal, peak_time)
            areas.append(area)
        
        return areas
    
    def calculate_retention_times(self, peaks: np.ndarray = None) -> List[float]:
        """
        Calculate retention times for peaks.
        
        Args:
            peaks: Peak indices (if None, uses previously found peaks)
            
        Returns:
            List of retention times
        """
        if peaks is None:
            if self.peaks is None:
                raise ValueError("No peaks found. Run find_peaks() first.")
            peaks = self.peaks
        
        return self.data['time'].iloc[peaks].tolist()
    
    def estimate_baseline(self, method: str = 'linear') -> pd.Series:
        """
        Estimate baseline of the chromatogram.
        
        Args:
            method: Method for baseline estimation ('linear', 'polynomial')
            
        Returns:
            Estimated baseline as pandas Series
        """
        signal = self.data['signal']
        time = self.data['time']
        
        if method == 'linear':
            # Simple linear baseline from start to end
            start_val = np.mean(signal[:10])  # Average of first 10 points
            end_val = np.mean(signal[-10:])   # Average of last 10 points
            baseline = np.linspace(start_val, end_val, len(signal))
        
        elif method == 'polynomial':
            # Polynomial fit to lower envelope
            # Find local minima
            window = len(signal) // 20
            minima_indices = []
            for i in range(window, len(signal) - window, window):
                local_min_idx = np.argmin(signal[i-window:i+window]) + i - window
                minima_indices.append(local_min_idx)
            
            # Fit polynomial to minima
            if len(minima_indices) > 3:
                poly_coeffs = np.polyfit(time.iloc[minima_indices], 
                                       signal.iloc[minima_indices], 2)
                baseline = np.polyval(poly_coeffs, time)
            else:
                # Fall back to linear if not enough minima
                baseline = np.linspace(signal.iloc[0], signal.iloc[-1], len(signal))
        
        self.baseline = pd.Series(baseline, index=self.data.index)
        self.data['baseline'] = self.baseline
        return self.baseline
    
    def correct_baseline(self) -> pd.Series:
        """
        Perform baseline correction on the signal.
        
        Returns:
            Baseline-corrected signal
        """
        if self.baseline is None:
            self.estimate_baseline()
        
        corrected_signal = self.data['signal'] - self.baseline
        self.data['corrected_signal'] = corrected_signal
        return corrected_signal
    
    def generate_report(self) -> dict:
        """
        Generate a comprehensive analysis report.
        
        Returns:
            Dictionary containing analysis results
        """
        # Ensure analysis is complete
        self.smooth_signal()
        peaks, peak_props = self.find_peaks()
        areas = self.calculate_peak_areas(peaks)
        retention_times = self.calculate_retention_times(peaks)
        self.estimate_baseline()
        
        report = {
            'timestamp': datetime.now().isoformat(),
            'total_peaks': len(peaks),
            'total_runtime': self.data['time'].iloc[-1] - self.data['time'].iloc[0],
            'peaks': []
        }
        
        # Add individual peak information
        for i, (peak_idx, area, rt) in enumerate(zip(peaks, areas, retention_times)):
            peak_info = {
                'peak_number': i + 1,
                'retention_time': rt,
                'peak_height': self.data['signal'].iloc[peak_idx],
                'peak_area': area,
                'peak_width': peak_props.get('widths', [0])[i] if i < len(peak_props.get('widths', [])) else 0
            }
            report['peaks'].append(peak_info)
        
        # Add system statistics
        report['system_stats'] = {
            'avg_pressure': self.data['pressure'].mean(),
            'pressure_std': self.data['pressure'].std(),
            'avg_temperature': self.data['temperature'].mean(),
            'temperature_std': self.data['temperature'].std(),
            'signal_range': self.data['signal'].max() - self.data['signal'].min(),
            'signal_noise': self.data['signal'].std()
        }
        
        return report
    
    def plot_chromatogram(self, show_peaks: bool = True, show_baseline: bool = True, 
                         figsize: Tuple[int, int] = (12, 8)) -> plt.Figure:
        """
        Plot the chromatogram with optional peak and baseline overlays.
        
        Args:
            show_peaks: Whether to mark detected peaks
            show_baseline: Whether to show baseline
            figsize: Figure size as (width, height)
            
        Returns:
            Matplotlib figure object
        """
        fig, (ax1, ax2, ax3) = plt.subplots(3, 1, figsize=figsize, sharex=True)
        
        # Main chromatogram
        ax1.plot(self.data['time'], self.data['signal'], 'b-', label='Signal', linewidth=1)
        
        if 'smoothed_signal' in self.data.columns:
            ax1.plot(self.data['time'], self.data['smoothed_signal'], 'r-', 
                    label='Smoothed', linewidth=2, alpha=0.7)
        
        if show_baseline and self.baseline is not None:
            ax1.plot(self.data['time'], self.baseline, 'g--', label='Baseline', alpha=0.7)
        
        if show_peaks and self.peaks is not None:
            peak_times = self.data['time'].iloc[self.peaks]
            peak_signals = self.data['signal'].iloc[self.peaks]
            ax1.plot(peak_times, peak_signals, 'ro', markersize=8, label='Peaks')
        
        ax1.set_ylabel('Signal Intensity')
        ax1.set_title('Chromatogram')
        ax1.legend()
        ax1.grid(True, alpha=0.3)
        
        # Pressure trace
        ax2.plot(self.data['time'], self.data['pressure'], 'purple', linewidth=1)
        ax2.set_ylabel('Pressure (bar)')
        ax2.set_title('System Pressure')
        ax2.grid(True, alpha=0.3)
        
        # Temperature trace
        ax3.plot(self.data['time'], self.data['temperature'], 'orange', linewidth=1)
        ax3.set_ylabel('Temperature (°C)')
        ax3.set_xlabel('Time (min)')
        ax3.set_title('Column Temperature')
        ax3.grid(True, alpha=0.3)
        
        plt.tight_layout()
        return fig


def analyze_chromatography_file(filename: str) -> dict:
    """
    Convenience function to analyze a chromatography data file.
    
    Args:
        filename: Path to CSV file containing chromatography data
        
    Returns:
        Analysis report dictionary
    """
    # Load data
    data = pd.read_csv(filename)
    
    # Create analyzer and generate report
    analyzer = ChromatographyAnalyzer(data)
    report = analyzer.generate_report()
    
    # Save plot
    fig = analyzer.plot_chromatogram()
    plot_filename = filename.replace('.csv', '_chromatogram.png')
    fig.savefig(plot_filename, dpi=300, bbox_inches='tight')
    plt.close(fig)
    
    print(f"Analysis complete. Plot saved as {plot_filename}")
    return report


# Example usage
if __name__ == "__main__":
    # Generate example data for demonstration
    time = np.linspace(0, 20, 1000)  # 20 minutes
    
    # Simulate chromatogram with multiple peaks
    signal = np.zeros_like(time)
    
    # Add peaks at different retention times
    peaks_rt = [5, 8.5, 12, 15.5]
    peaks_height = [2.0, 1.5, 3.0, 1.0]
    peaks_width = [0.3, 0.4, 0.25, 0.5]
    
    for rt, height, width in zip(peaks_rt, peaks_height, peaks_width):
        signal += height * np.exp(-0.5 * ((time - rt) / width) ** 2)
    
    # Add baseline drift and noise
    baseline = 0.1 + 0.05 * time + 0.02 * np.sin(0.5 * time)
    noise = np.random.normal(0, 0.05, len(time))
    signal += baseline + noise
    
    # Simulate system parameters
    pressure = 150 + 10 * np.sin(0.1 * time) + np.random.normal(0, 2, len(time))
    temperature = 40 + 2 * np.sin(0.05 * time) + np.random.normal(0, 0.5, len(time))
    
    # Create DataFrame
    data = pd.DataFrame({
        'time': time,
        'signal': signal,
        'pressure': pressure,
        'temperature': temperature
    })
    
    # Analyze the data
    analyzer = ChromatographyAnalyzer(data)
    report = analyzer.generate_report()
    
    print("Chromatography Analysis Report")
    print("=" * 40)
    print(f"Total peaks found: {report['total_peaks']}")
    print(f"Total runtime: {report['total_runtime']:.2f} minutes")
    print("\nPeak Information:")
    for peak in report['peaks']:
        print(f"  Peak {peak['peak_number']}: RT={peak['retention_time']:.2f}min, "
              f"Height={peak['peak_height']:.3f}, Area={peak['peak_area']:.2f}")
    
    # Generate plot
    fig = analyzer.plot_chromatogram()
    plt.show()
