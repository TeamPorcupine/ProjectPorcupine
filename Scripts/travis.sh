#! /bin/sh
# This script is responsible for running the script that matches
# the environment variable travis selects.

if [ "$JOB" == "unit-test" ]; then
   ./Scripts/test.sh --travis
   exit $?
fi


if [ "$JOB" == "stylecop" ]; then
    ./Scripts/check-style.sh
fi