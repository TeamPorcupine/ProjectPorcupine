echo 'travis_fold:start:stylecop'
echo "Stylecopping"
StylecopOutput=$(mono /opt/stylecop/StyleCopCmd.Console.exe -rd Assets/ -vt)
StyleCopErrorCode=$?

if [ "$StyleCopErrorCode" == "0" ]; then
    echo '\nNo Stylecop violations were found.\n'
fi
echo 'travis_fold:end:stylecop'


if [ "$StyleCopErrorCode" != "0" ]; then
    echo '\nThe following Stylecop violations were thrown:\n'
    echo "$StylecopOutput\n"
    exitStatus=1
fi
