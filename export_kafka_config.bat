@echo off
:: Set Kafka and ksqlDB container names
set KAFKA_CONTAINER_NAME=broker
set KSQLDB_CONTAINER_NAME=ksqldb-server

:: Set temporary paths inside containers
set KAFKA_TMP_PATH=/tmp
set KSQLDB_TMP_PATH=/tmp

:: Set output directory on host
set OUTPUT_DIR=./kafka_exports

:: Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
    if errorlevel 1 (
        echo Failed to create directory %OUTPUT_DIR%. Exiting.
        pause
        exit /b
    )
)

echo Exporting Kafka topics and configurations...

:: Export topics list
docker exec %KAFKA_CONTAINER_NAME% kafka-topics --bootstrap-server localhost:9092 --list > "%OUTPUT_DIR%/topics_list.txt"
if errorlevel 1 (
    echo Failed to export Kafka topics list. Exiting.
    pause
    exit /b
)

:: Export topics configuration
if exist "%OUTPUT_DIR%/topics_list.txt" (
    for /F "tokens=*" %%T in (%OUTPUT_DIR%/topics_list.txt) do (
        docker exec %KAFKA_CONTAINER_NAME% kafka-configs --bootstrap-server localhost:9092 --entity-type topics --entity-name %%T --describe >> "%OUTPUT_DIR%/topics_config.txt"
        if errorlevel 1 (
            echo Failed to describe topic %%T. Continuing to next topic...
        )
    )
) else (
    echo Topics list file not found. Exiting.
    pause
    exit /b
)

echo Exporting consumer groups...
docker exec %KAFKA_CONTAINER_NAME% kafka-consumer-groups --bootstrap-server localhost:9092 --list > "%OUTPUT_DIR%/consumer_groups.txt"
if errorlevel 1 (
    echo Failed to export consumer groups. Exiting.
    pause
    exit /b
)

echo Exporting ksqlDB streams and queries...

:: Export streams
docker exec %KSQLDB_CONTAINER_NAME% curl -X POST http://localhost:8088/ksql ^
    -H "Content-Type: application/vnd.ksql.v1+json" ^
    -d "{\"ksql\": \"SHOW STREAMS;\"}" > "%OUTPUT_DIR%/streams.txt"
if errorlevel 1 (
    echo Failed to export ksqlDB streams. Exiting.
    pause
    exit /b
)

:: Export queries
docker exec %KSQLDB_CONTAINER_NAME% curl -X POST http://localhost:8088/ksql ^
    -H "Content-Type: application/vnd.ksql.v1+json" ^
    -d "{\"ksql\": \"SHOW QUERIES;\"}" > "%OUTPUT_DIR%/queries.txt"
if errorlevel 1 (
    echo Failed to export ksqlDB queries. Exiting.
    pause
    exit /b
)

:: Export extended details of streams and tables
if exist "%OUTPUT_DIR%/streams.txt" (
    for /F "tokens=*" %%S in (%OUTPUT_DIR%/streams.txt) do (
        docker exec %KSQLDB_CONTAINER_NAME% curl -X POST http://localhost:8088/ksql ^
            -H "Content-Type: application/vnd.ksql.v1+json" ^
            -d "{\"ksql\": \"DESCRIBE EXTENDED %%S;\"}" >> "%OUTPUT_DIR%/stream_descriptions.txt"
        if errorlevel 1 (
            echo Failed to describe stream %%S. Continuing to next stream...
        )
    )
) else (
    echo Streams file not found. Skipping detailed descriptions.
)

echo Export process complete. All files saved in %OUTPUT_DIR%.
pause
