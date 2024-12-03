@echo off
:: Set Kafka and ksqlDB container names
set KAFKA_CONTAINER_NAME=broker
set KSQLDB_CONTAINER_NAME=ksqldb-server

:: Set input directory for exported files
set INPUT_DIR=./kafka_exports

:: Check if the input directory exists
if not exist "%INPUT_DIR%" (
    echo Input directory %INPUT_DIR% does not exist. Exiting.
    pause
    exit /b
)

echo Importing Kafka topics...

:: Import Kafka topics and configurations
if exist "%INPUT_DIR%/topics_config.txt" (
    for /F "tokens=1,2,3,* delims= " %%A in (%INPUT_DIR%/topics_config.txt) do (
        if "%%A"=="Configs" (
            echo Creating topic %%B with configuration: %%C %%D
            docker exec %KAFKA_CONTAINER_NAME% kafka-topics --bootstrap-server localhost:9092 ^
                --create --topic %%B --if-not-exists ^
                --config %%C=%%D
        )
    )
) else (
    echo Topics configuration file not found. Skipping topics import.
)

echo Importing consumer groups...

:: Import consumer groups (not supported for automatic recreation; requires manual consumer restart)
echo Consumer groups cannot be recreated automatically. Ensure consumers are restarted with the correct configurations.

echo Importing ksqlDB streams and queries...

:: Import ksqlDB streams
if exist "%INPUT_DIR%/streams.txt" (
    for /F "tokens=*" %%S in (%INPUT_DIR%/streams.txt) do (
        echo Recreating stream: %%S
        docker exec %KSQLDB_CONTAINER_NAME% curl -X POST http://localhost:8088/ksql ^
            -H "Content-Type: application/vnd.ksql.v1+json" ^
            -d "{\"ksql\": \"%%S\"}"
    )
) else (
    echo Streams file not found. Skipping stream import.
)

:: Import ksqlDB queries
if exist "%INPUT_DIR%/queries.txt" (
    for /F "tokens=*" %%Q in (%INPUT_DIR%/queries.txt) do (
        echo Recreating query: %%Q
        docker exec %KSQLDB_CONTAINER_NAME% curl -X POST http://localhost:8088/ksql ^
            -H "Content-Type: application/vnd.ksql.v1+json" ^
            -d "{\"ksql\": \"%%Q\"}"
    )
) else (
    echo Queries file not found. Skipping queries import.
)

echo Import process complete.
pause
