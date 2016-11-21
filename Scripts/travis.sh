#! /bin/sh

if [ "$TEST_SUITE" == "unit-test" ]; then
    ./Scripts/test.sh --travis
fi


if [ "$TEST_SUITE" == "stylecop" ]; then
    ./Scripts/check-style.sh
fi