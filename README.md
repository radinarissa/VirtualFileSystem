# Virtual File System

This is my implementation of a virtual file system that simulates file operations on a binary container file. The project was created as a course assignment.

## Assignment Requirements

The task was to implement a program that simulates a file system on a binary file container. The program must get its parameters from the command line, and operations on the file system must be immediately reflected in permanent memory.

### Implemented Features

1. **File Operations**
   - "cpin" - Copying a file into the container
   - "ls" - Listing the container contents
   - "rm" - Deleting a file from the container
   - "cpout" - Copying a file from the container to the external file system

2. **Directory Operations**
   - "md" - Creating a directory with a specific name in the current container directory
   - "cd" - Changing the current directory
   - "rd" - Deleting a directory with a specific name in the current container directory

## Implementation Details

- I've implemented the file system using block-based allocation with a bitmap to track free and used blocks
- Files and directories are stored in a container with metadata that includes name, size, and location information
- The system supports a hierarchical directory structure with navigation capabilities
- I've created my own implementations of data structures (linked lists, stacks) as required by the assignment

## My Approach

For this project, I chose to use a block-based allocation system where:
- The container file is divided into fixed-size blocks
- A bitmap tracks which blocks are free or used
- Metadata is stored at the beginning of the container
- Files can be of variable sizes, utilizing multiple blocks when necessary

All the operations (create, read, delete) directly manipulate the container file without loading the entire structure into memory, as required by the assignment.

## How to Run

The program uses a simple menu-based interface with numbered options for the various file system operations.
