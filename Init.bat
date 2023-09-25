@echo off
REM Logging the start of the process
echo Starting git operations...

REM Delete the .git directory
echo Removing existing repository...
IF EXIST ".git" (
    rmdir /s /q .git
)

REM Initialize a new Git repository
echo Initializing new repository... 
git init

REM Setting up the remote repository
echo Setting up remote repository...
git remote add origin https://github.com/StevenRotelli/GrooperGit

REM Pulling latest changes from master
echo Pulling latest changes...
git pull origin master

echo Done!
 