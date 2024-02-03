# Steps taken to refactor the itu-minitwit app to Python 3 + including collaboration best practices:

## Migrating to Python3:
- Migrate ```minitwit.py``` from Python 2 to Python 3 was done running ```2to3 minitwit.py```
- Migrate ```minitwit_tests.py``` from Python 2 to Python 3 was done running ```2to3 minitwit_tests.py```

## Python project collaboration best practices:
### Use Python virtual environments
- Inside the itu-minitwit folder ```python3 -m venv minitwitvenv``` command was executed to create the virtual environment.
- ```source venv/bin/activate``` command was executed to activate the virtual environment.

### Dependency versioning
- The correct use of virtual environments requires creating a ```requirements.txt``` file so we can all share same dependencies versions and avoid versioning issues.
- All dependencies were included inside the ```requirements.txt``` file by running ```pip freeze > requirements.txt```. This file was commited to the repo as it is a way of centralising dependency management across team members. It is important that we work with the same dependencies to avoid any potential compatibility issue.

## TODO: it is suggested that all team members should run the following commands locally to align with the mentioned best practices.
### To create a virtual environment locally:
- Inside the itu-minitwit folder run ```python3 -m venv minitwitvenv``` to create the virtual environment.
- Run ```source venv/bin/activate``` to activate the virtual environment.
- Run ```pip install -r requirements.txt``` so we all have the same dependencies.
- The virtual environment must be named ```minitwitvenv``` because this folder name was included in the .gitignore file. Virtual environments must never be commited to the repository, these are files that are kept locally only.

### Every time we want to add a new dependency:
- Run ```pip install <package_name>```.
- Commit ```requirements.txt``` into the repo so we can all have the updated version.

### Every time we pull changes from main:
- Run ```pip install -r requirements.txt``` so we all have the same dependencies.


