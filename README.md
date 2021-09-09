# NOTE
I Made This Tool In 4 Days Send Help ;-; \
I Used Visual Studio 2013 To Make This Since My Computer Is A Potato. \
**IMPORTANT: PVZ is not mine or associated with this tool, \
PLEASE DO NOT HACK PEOPLE USING MY CODE!**
# You Should Not:
* Keylog
* Forkbomb
* Corrupt Windows Memory
* Delete Windows
* Infect Files
* Embed Malicious DLLs
# Explanations & Resources
'Keylog' - My app specifically does not contain a 'Keylogger' however, \
c# doesn't support keylogging and that is why i bound it with c++ DLLs thanks to this guy's code: \
https://github.com/crhenr/csharp-keylogger/blob/master/KeyLogger/KeyLogger/Program.cs \
If you are asking why i made it do this, \
It is so you can toggle the cheats even if you aren't typing in the application \
like typing the toggle command while you are in PVZ instead of pausing the game and toggling the cheat from the tool. \
\
I found some codes from 'PVZTools' so big thanks to you: https://github.com/lmintlcx/pvztools \
NuGet was being the worst nugget in history so i couldn't get memory.dll anyway. Thanks 'Guided Hacking': \
https://www.youtube.com/watch?v=STPrGJ8eI8Y \
\
For the automatic PVZ locator i made, i wanna thank 'IAmTimCorey' for talking about 'System.IO' in c#: \
https://www.youtube.com/watch?v=9mUuJIKq40M \
\
I actually also tried using c++ for dll injection in the game but ended up not doing it since \
it crashed my game for no reason so i couldn't use it for the memory stuff. \
So then i decided to use Node.JS big thanks to node: \
https://nodejs.org/en/ \
and then i found this library called memoryjs big thanks to this library \
(or, i don't know since it was useless and didn't work properly) \
https://www.npmjs.com/package/memoryjs \
and i also made user input using inquirer big thanks to inquirer: \
https://www.npmjs.com/package/inquirer \
\
Then i wasted the 2nd day with memoryjs and it just wasted my time. \
I got so disappointed so i then moved to c# on the 3rd day and then \
learned 'System.IO' from 'IAmTimCorey' then... you get the point, it was the resources i gave. \
Since i couldn't get memory.dll to work because NuGet is stupid, i decided to find an alternative. \
Then 'Guided Hacking' helped me again but this time with pinvoke: \
https://www.youtube.com/watch?v=VKaudl5_3w8 \
http://pinvoke.net/ \
\
But then, it didn't work properly as well. \
But, thanks to till0sch i managed to make it work!: \
https://guidedhacking.com/members/till0sch.6392/ \
\
And for the 'Key checking' thing, i made a dll for that then embed it into the project. \
That's what i meant by 'Embed Malicious DLLs'. \
\
'Forkbomb' - people can forkbomb with c# anyway. \
The reason i said 'Forkbomb' is because i have code that launches PVZ so, you probably get the point. \
\
'Corrupt Windows Memory' - because you can modify process memory with this since it is basically cheating PVZ \
and that basically means that when you make this launch at startup and do a malicious command \
that causes the computer to crash then... please don't ;-;. \
\
'Infect Files' - Filesystem again. \
'Delete Windows' - SAME CASE, FILESYSTEM. \
And that is basically what happened. \
I tried making a messagebox when it hooked but it was hard and i figured it might be annoying. \
I might add command line optons soon so you can make a shortcut and that it just doesn't get in the way with the prompts. \
I was gonna use Sharprompt but as i said before, NuGet is stupid. \
Here is Sharprompt: \
https://github.com/shibayan/Sharprompt \
\
If you are wondering why the app needs administrator privileges, \
it's because it needs to access some DLL libraries for the process memory modification of PVZ. \
Oh, and i almost forgot, this only supports the **`OLD VERSION`** of PVZ BUT but but, \
I might add support for the **`GOTY EDITION`** \
\
And yeah, Visual Studio 2013. \
If you don't want to download the release then you can always check my code and compile. \
I have nothing to hide. \
There is no mac & linux support or something. \
(I don't even know if there is a linux version of PVZ) \
But that is basically it. \
**Usage (Type Anywhere): tog(number of the cheat you want to toggle)** \
Big thanks to minecraft for the sound effect (literally just got the emerald block note block sfx): https://minecraft.net/
# Have Fun!
Seriously, have fun.
