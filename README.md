RandomIG - Random Image Generator
-----
RandomIG is a random image generator that takes a function as a random seed.

In order to compile it, you need this expression parsing library: 
http://www.bestcode.com/assets/docs/bcParserNET/solution_bcparser.net.htm

Help
-----

Seed: This is the expression that takes (x, y) arguments (image co-ordinates), in order to generate random seed.

PSeed: This is the expression that will be evaluated on the previous and the current seed, and it's used instead of the new seed. (x is the old seed, y is the new seed)

Screenshots
-------------

The following is a generated image of `(x*x) + (y*y)`.

![screenshot](https://github.com/diantahoc/RandomIG/raw/master/misc/shot.png "screenshot")