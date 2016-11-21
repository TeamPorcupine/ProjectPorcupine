#! /bin/sh

if [ "$TEST_SUITE" == "unit-test" ]; then
    ./test.sh --travis
fi


if [ "$TEST_SUITE" == "stylecop" ]; then
    ./check-style.sh
fi