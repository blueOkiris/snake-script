# Snake Script

A statically-typed stack-based esoteric programming language that is made to be used in 80x25 consoles but is also (hopefully) useful

## How to use

## Build

You can build the interpreter 'snakey' by running:

 * `make LINUX=1` for Linux
 
 * `mingw32-make WIN32=1` for Windows 32-bit
 
 * `ming32-make WIN64=1` for Windows 64-bit

## Running

Once the snakey compiler/interpreter is built, you can run a snake script program with the command:

`snakey <filename>` (ex: `snakey examples/factorial.snake`)

There are a few examples in the examples folder

### Types

Snake Script is designed for 80x25 character terminals, and as such every line must be 80 characters wide. No more, no less. Also, the lines must "snake," i.e. line one is written left-to-right, but line 2 is written right-to-left.

Beyond that restriction, the language is fairly normal.

You can push numbers, booleans, characters, and variables to a stack. You can also push lists and tuples to the stack. There is syntactic sugar for strings, but they are treated as character arrays.

How to push types:

 * Characters are any single character (including escape sequences) between single quotes
 
 * Numbers are any single number, decimal number, or decimal number in scientific notation
 
 * Booleans are represented by `?t` for true and `?f` for false
 
 * Lists are defined as `[`, any sequence of values *of the same type*, and then `]`
 
 * Strings can either be defined as a list of chars i.e. `['a''b''c' ]` *or* they can be defined using single quotes, like characters, but simply using more characters, i.e. `'abc'`
 
 * Tuples can be defined by `(`, any two values, and then `)`
 
 * Last it's good to note that you can define lists of lists or lists of tuples or lists of lists of tuples of ints and lists of characters and so on. It's recursive
 
 * Finally, variable references can be pushed by simply giving their name. By default variables have no value and will throw errors when used. You can set them and their type using the assignmnet operator `=`. Once a type has been set, however, *it cannot be changed*

### Functions

You can also define functions. All functions take a single input and a single output (though these can be lists and tuples for more values)

Functions are defined in the following manner:

`\ identifier : type1 > type2 { statements go here }`

The input and return types can be any of the ones mentioned above, however, they're denoted using the following syntax:

 * `#` for number
 
 * `@` for character
 
 * `??` for boolean
 
 * `[` `]` for lists
 
 * `(` `)` for tuples
 
So as an example, `[[(#([#](??,[@])))]]` would be a list of lists of tuples of numbers and tuples of lists of numbers and tuples of booleans and lists of characters.

### While Loops

The last main construct is the while loop.

After pushing a boolean value to the top of the stack, you can choose to enter a loop of statements or skip it.

So the syntax is:

`[?] { <statements> }`

See the truth machine example to understand this in full

### Statements

Finally, everything else is a statement. Typically these pop one or two values off of the stack and push something back.


