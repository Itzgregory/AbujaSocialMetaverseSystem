import os
import re
from datetime import datetime

DOC_DIR = os.path.join(os.getcwd(), 'doc')
TODAY = datetime.utcnow().strftime('%Y-%m-%d')

def process_markdown_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    if not lines:
        return
        
    # Check if already processed
    if any(line.startswith('Title:') for line in lines[:15]):
        print(f"Skipping {os.path.basename(filepath)} - Already has metadata")
        return

    title = ""
    summary = ""
    
    # Extract Title (first H1)
    for line in lines:
        if line.startswith('# '):
            title = line[2:].strip()
            break
            
    if not title:
        # Fallback to filename without extension
        title = os.path.splitext(os.path.basename(filepath))[0].replace('-', ' ').title()
        
    # Extract summary (first non-empty, non-header line after title)
    for line in lines:
        stripped = line.strip()
        if stripped and not stripped.startswith('#') and not stripped.startswith('![') and not stripped.startswith('<') and not stripped.startswith('---') and not stripped.startswith('```'):
            # Only use if it's a decent length sentence
            if len(stripped) > 20:
                summary = stripped
                # Truncate summary if too long
                if len(summary) > 150:
                    summary = summary[:147] + "..."
                break
                
    if not summary:
        summary = f"Documentation for {title}"

    filename = os.path.basename(filepath)
    
    metadata = f"""```
Title: {title}
Doc ID / filename: {filename}
Status: Active
Author(s): Antigravity
Owner: Gregory Opara
Engineering Owner: Gregory Opara
QA Owner: Gregory Opara
Ops Owner: Gregory Opara
Created: {TODAY}
Last updated: {TODAY}
Related Epic / Ticket(s): N/A
Short summary: {summary}
Contact: oparagregory
```

**TL;DR:** {summary}

"""

    change_history = f"""

---

## Change History
- v1.0 – {TODAY} – Applied Elios Technology Documentation Standards (Antigravity)
"""

    # Combine everything
    # We put metadata at the very top.
    new_content = metadata + "".join(lines) + change_history
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)
    print(f"Updated {filename}")

if __name__ == '__main__':
    count = 0
    for root, dirs, files in os.walk(DOC_DIR):
        for file in files:
            if file.endswith('.md'):
                process_markdown_file(os.path.join(root, file))
                count += 1
                
    print(f"Processed {count} files.")
