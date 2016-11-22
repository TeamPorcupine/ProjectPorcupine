echo "Stylecopping"
StylecopOutput=$(mono /opt/stylecop/StyleCopCmd.Console.exe -rd Assets/ -vt)
StyleCopErrorCode=$?

if [ "$StyleCopErrorCode" == "0" ]; then
    echo '\nNo Stylecop violations were found.\n'
fi


if [ "$StyleCopErrorCode" != "0" ]; then
    echo '\nThe following Stylecop violations were thrown:\n'
    echo "$StylecopOutput\n"
    exit 1
fi
