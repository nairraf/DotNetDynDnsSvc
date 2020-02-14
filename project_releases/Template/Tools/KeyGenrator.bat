@ECHO OFF

SET CONFIG="..\Conf\configuration.yaml"
SET KEYGEN="KeyGenerator\KeyGenerator.exe"
doskey

ECHO.
ECHO Note: You can use %%CONFIG%% as the config file and %%KEYGEN%% as the keygen executable
ECHO 	Example: %%KEYGEN%% config=%%CONFIG%% help

cmd /k %KEYGEN% config=%CONFFIG% help

