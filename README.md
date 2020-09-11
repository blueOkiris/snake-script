# Snake Script

A statically-typed stack-based esoteric programming language that is made to be used in 80x25 consoles but is also (hopefully) useful

Here's an example 99-bottles of beer on the wall program to illustrate how it works. Note you read left-to-right, right-to-left, and so on:

```
\brwl:#>[@]{$' bottles of beer on the wall.\n'++<<}\br:#>[@]{$                  
'.reeb fo selttob 99n\.llaw eht no reeb fo selttob 99'}<<++'n\.reeb fo selttob '
'\nTake one down pass it around.\n'++. 98i=i2?>[?]{i1-i=i(brwl)['\n']++i(brwl)++
'.llaw eht no reeb fo elttob 1'}>?2i.++'n\.dnuora ti ssap nwod eno ekaT' ++)rb(i
'\n\n'++'1 bottle of beer on the wall.\n1 bottle of beer.\n'++                  
      .++'n\.llaw eht no reeb fo selttob erom oNn\.dnuora ti ssap nwod eno ekaT'
```

## Build

You can build the interpreter, 'snakey,' by running:

 * `make LINUX=1` for Linux
 
 * `mingw32-make WIN32=1` for Windows 32-bit
 
 * `ming32-make WIN64=1` for Windows 64-bit

## Running

Once the snakey compiler/interpreter is built, you can run a snake script program with the command:

`snakey <filename>` (ex: `snakey examples/factorial.snake`)

There are a few examples in the examples folder

You can also print debug information like tokenizer output, AST, and generated bytecode by adding the tag `--debug` i.e. `snakey <filename> --debug`

## How to use

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
 
So as an example, `[[(#([#](??[@])))]]` would be a list of lists of tuples of numbers and tuples of lists of numbers and tuples of booleans and lists of characters.

### While Loops

The last main construct is the while loop.

After pushing a boolean value to the top of the stack, you can choose to enter a loop of statements or skip it.

So the syntax is:

`[?] { <statements> }`

See the truth machine example to understand this in full

### Statements

Finally, everything else is a statement. Typically these pop one or two values off of the stack and push something back.

| Symbol | Description |
|:------:|:-----------:|
| `<<` | Pop an item off the stack, return from a function, and push the value back |
| `>>` | Pop an item off the stack |
| `><` | Duplicate the current item on the stack |
| `<>` | Swap the top two items on the stack
| `++` | Pop list 2, pop list 1, push list 1 concatenated with list 2 (Note stack order!) |
| `--` | Pop number, pop list, remove from list at index number |
| `@@` | Pop number, pop list, push list, push list\[number\] |
| `][` | Pop list, push items (in reverse order) onto stack. "Unzip" if you will |
| `[]` | Pop all items of the same type, push a list of those items. "Zip" if you will |
| `^^` | Pop a number off the stack, push that number rounded to the closest integer |
| `.` | Pop an item off the stack, print its value |
| `,` | Take user input, push it to the stack as a string
| `+` | Pop number 2, pop number 1, push number 1 + number 2 |
| `-` | Pop num 2, pop num 1, push num 1 - num 2 |
| `*` | Pop num 2, pop num 1, push num 1 * num 2 |
| `/` | Pop num 2, pop num 1, push num 1 / num 2 |
| `^` | Pop num 2, pop num 1, push (num 1) ^ (num 2) |
| `?>` | Pop val 2, pop val 1, push val 1 > val 2 |
| `?<` | Pop val 2, pop val 1, push val 1 > val 2 |
| `?=` | Pop val 2, pop val 1, push val 1 == val 2 |
| `?!` | Pop bool, push not bool |
| `?&` | Pop bool 2, pop bool 1, push bool 1 && bool 2 |
| `?\|` | Pop bool 2, pop bool 1, push bool 1 || bool 2 |
| `=` | Pop identifier, pop value, set identifier == value (throws error if not the same type) |
| `()` | Pop val 2, pop val 1, push `( val1 val2 )` |
| `$` | Pop value, push that value converted to string |
| \` | Pop number, push that number as a character |
| `!?` | Pop string, push extracted value. "Parse," if you will |
| `?` | Pop number or character, push boolean value of them (like in C) |
