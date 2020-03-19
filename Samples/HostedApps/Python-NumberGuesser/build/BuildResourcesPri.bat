@ECHO OFF
REM MakePri.exe createconfig /cf .\priconfig.xml /dq lang-en-US /o /pv 10.0.0
REM
REM - to get a single Resources.pri, open the priconfig.xml and remove the <packaging> section
REM
ECHO Checking whether to delete existing resources.pri
IF EXIST ".\NumberGuesser\resources.pri" (
	DEL ".\NumberGuesser\resources.pri" 
	ECHO .\NumberGuesser\resources.pri deleted
)
MakePri.exe new /cf .\priconfig.xml /pr ..\NumberGuesser /mn ..\NumberGuesser\AppXManifest.xml /o /of ..\NumberGuesser\resources.pri
