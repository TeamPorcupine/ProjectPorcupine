#! /bin/sh
# This script is responsible for running the script that matches
# the environment variable travis selects.

if [ "$JOB" == "unit-test" ]; then
   
   ./Scripts/Install/unity.sh
   ./Scripts/test.sh --travis
   exit $?
fi


if [ "$JOB" == "stylecop" ]; then
	./Scripts/Install/mono.sh
	./Scripts/Install/stylecop.sh
    ./Scripts/check-style.sh
fi