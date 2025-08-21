#!/usr/bin/env python3
"""
PDF Text Extractor for Waters Documentation

This script extracts text from Waters Acquity PDF documentation files
to help analyze the command specifications.
"""

import PyPDF2
import os
import sys

def extract_pdf_text(pdf_path):
    """
    Extract text from a PDF file.
    
    Args:
        pdf_path: Path to the PDF file
        
    Returns:
        Extracted text as string
    """
    try:
        with open(pdf_path, 'rb') as file:
            pdf_reader = PyPDF2.PdfReader(file)
            text = ""
            
            print(f"Processing {len(pdf_reader.pages)} pages...")
            
            for page_num, page in enumerate(pdf_reader.pages):
                try:
                    page_text = page.extract_text()
                    text += f"\n--- Page {page_num + 1} ---\n"
                    text += page_text
                except Exception as e:
                    print(f"Error extracting page {page_num + 1}: {e}")
                    continue
            
            return text
            
    except Exception as e:
        print(f"Error reading PDF: {e}")
        return None

def search_commands_in_text(text, search_terms=None):
    """
    Search for command-related content in the extracted text.
    
    Args:
        text: Extracted PDF text
        search_terms: List of terms to search for
        
    Returns:
        Dictionary of found command sections
    """
    if search_terms is None:
        search_terms = [
            'command', 'CMD', 'protocol', 'communication',
            'FLOW', 'TEMP', 'RUN', 'START', 'STOP', 'STATUS',
            'BSM', 'CM', 'SM', 'TUV', 'serial', 'TCP', 'ethernet',
            'automation', 'portal', 'ASCII', 'query', 'response'
        ]
    
    found_sections = {}
    lines = text.split('\n')
    
    for i, line in enumerate(lines):
        line_upper = line.upper()
        for term in search_terms:
            if term.upper() in line_upper:
                # Get surrounding context (5 lines before and after)
                start_idx = max(0, i - 5)
                end_idx = min(len(lines), i + 6)
                context = '\n'.join(lines[start_idx:end_idx])
                
                if term not in found_sections:
                    found_sections[term] = []
                found_sections[term].append({
                    'line_number': i + 1,
                    'content': line.strip(),
                    'context': context
                })
    
    return found_sections

def main():
    """Main function to process Waters PDF documentation."""
    pdf_dir = "docs/proprietary"
    
    if not os.path.exists(pdf_dir):
        print(f"Directory {pdf_dir} not found!")
        return
    
    pdf_files = [f for f in os.listdir(pdf_dir) if f.endswith('.pdf')]
    
    if not pdf_files:
        print(f"No PDF files found in {pdf_dir}")
        return
    
    print("Waters Acquity Documentation Analysis")
    print("=" * 50)
    
    for pdf_file in pdf_files:
        pdf_path = os.path.join(pdf_dir, pdf_file)
        print(f"\nProcessing: {pdf_file}")
        print("-" * 30)
        
        # Extract text
        text = extract_pdf_text(pdf_path)
        
        if text:
            # Save extracted text
            text_file = pdf_file.replace('.pdf', '_extracted.txt')
            text_path = os.path.join(pdf_dir, text_file)
            
            with open(text_path, 'w', encoding='utf-8') as f:
                f.write(text)
            print(f"Extracted text saved to: {text_path}")
            
            # Search for commands
            command_sections = search_commands_in_text(text)
            
            if command_sections:
                print(f"\nFound command-related content:")
                for term, sections in command_sections.items():
                    if sections:  # Only show terms that were found
                        print(f"\n{term.upper()} ({len(sections)} occurrences):")
                        for section in sections[:3]:  # Show first 3 occurrences
                            print(f"  Line {section['line_number']}: {section['content'][:100]}...")
            
            # Look for specific protocol patterns
            print(f"\nSearching for Waters automation protocol patterns...")
            
            # Common Waters command patterns
            patterns_to_find = [
                r'\*IDN\?', r'STATUS\?', r'ERROR\?',
                r'BSM:', r'CM:', r'SM:', r'TUV:',
                r'FLOW', r'TEMP', r'RUN:', r'METHOD:',
                r'START', r'STOP', r'ABORT'
            ]
            
            import re
            found_patterns = {}
            for pattern in patterns_to_find:
                matches = re.findall(pattern, text, re.IGNORECASE)
                if matches:
                    found_patterns[pattern] = len(matches)
            
            if found_patterns:
                print("Command patterns found:")
                for pattern, count in found_patterns.items():
                    print(f"  {pattern}: {count} occurrences")
            else:
                print("No standard Waters command patterns found in text.")
                print("This might indicate the PDF contains images or non-standard formatting.")
        
        else:
            print(f"Failed to extract text from {pdf_file}")
            print("This could be due to:")
            print("- PDF contains scanned images rather than searchable text")
            print("- PDF is encrypted or has extraction restrictions")
            print("- File format issues")

if __name__ == "__main__":
    main()
