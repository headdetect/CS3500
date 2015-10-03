# Spreadsheet
### By Brayden Lopez (October 1st, 2015)

## Designs
At first I was going to go with a simple, implement the dependency graph, implement the formulas.
Which is what I ended up going with, and it works.  
The project (PS4) is set up as so:
- Spreadsheet Project: This is the project that connects all the pieces together.
- Formula: This is the PS3 project that implements PS1 in a nice solid package.
- DependencyGraph: This is the PS4 project that is used to show which cells depend on which cells.
- Spreadhseet Tests: These are all the tests for the unified spreadsheet project.

## Notes:
Make sure when compiling this project, you compile against VS2015. There's a bug (or a non-implemented feature) in the older versions:

In VS 2013 and lower, a struct that has arguments in the constructor must call the default constructor before assigning properties. In VS15+, you do not need to call the default constructor.   
An example of what I'm talking about:

```
internal OperationToken(char token) { 
   Operation = token; 
} 
```

that is valid in VS15+, but not in lower versions. 
The following must be put in previous versions:

```
internal OperationToken(char token) : this() { 
   Operation = token; 
} 
```

Which is what I didn't have... and which caused many a headache.

## General Thoughts
I was banging my head (literally) when I make the connection that every cell didn't need a dependency graph. Just thought I should make that apparent.