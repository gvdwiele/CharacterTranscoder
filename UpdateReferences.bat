@echo off
echo CommonReferencesFolder: %CommonReferencesFolder%
for /f "tokens=*" %%a in (references.txt) do (
    mkdir lib
    copy "%CommonReferencesFolder%%%a" /D "lib" /Y
    echo copied "%CommonReferencesFolder%%%a" 
)