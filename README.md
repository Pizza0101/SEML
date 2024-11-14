# SEML
SEML stands for "SEML Ein't a Markup Language".

It parses a highly simplified YAML-like syntax and is designed to handle complex configurations, i.e. from a block's CustomData.

## Features

- Nestable Dictionaries (string key, object value) and lists (object)
- Values as string, bool, or number (stored as double)

## Parsing logic

- An exception is thrown if indentation is not a multiple of 4 spaces (4 spaces equal one TAB in Space Engineers).
- An exception is thrown if indentation increases while the previous line was not a key-only line ('MyKey:' or "- MyKey:").
- Any line that is not part of a multi-line string but contains only whitespace or starts with a # (comment) is skipped. Comments at the end of a line are not supported.
- Only a dict or a list can exist at the same indentation level, not both. Either can be nested. If an indentation level starts with a dict and switches to a list (or vice versa), an exception will be thrown.
- Values can be strings, numbers, or bools. Strings must be enclosed in quotes (") and can span multiple lines. Any indentation in a multi-line string will be considered part of the string. If the value is neither a string, number, nor bool, an exception will be thrown.

## Example SEML
```
# Example Configuration
active: false
defaults:
    font:
        type: "Monospace"
        size: 2.8
        color:
            r: 255
            g: 128
            b: 0
    background_color: [0, 0, 0]
texts:
    - "This is the first text"
    - "This is another text"
    - "This is a multi-
line text"
    - text_source: 
        block: "My block with lots of CustomData"
        type: "CustomData"
    - "Text 5"
    - configured_text:
        text: "Text 6"
        font_size: 4.5
    - "Text 7"
```

