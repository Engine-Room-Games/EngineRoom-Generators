This document is created before any code was written in the project.
This project intended to be a source generators project.
Inside `docs` directory you can find all of the relevant documentations - coding conventions, etc. Note that coding convention file contains conventions both for unity and non-unity projects.
Inside `src` there are two directories. `generators-unity` which contains unity project that will have tests and inside `generators-src` there is the source code for roslyn source generators.
When creating generators, templates should be placed as text resources in the dll for easier maintanence.
Placeholders inside those templates should be wrapped with double percent symbols (e.g. `%%ClassName%%`).
The project will contain multiple generators so while developing a generator if it uses something other future generators can benefit from, this method should go into helpers class straight away.
The code should reuse as much as possible.
Prefer simplicity in code. Don't overengineer, but don't make a mess of the code either.