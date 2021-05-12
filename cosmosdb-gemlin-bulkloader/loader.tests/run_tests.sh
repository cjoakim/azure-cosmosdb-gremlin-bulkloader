#!/bin/bash

# Execute the Xunit unit tests (on linux/macos), then produce code coverage reports 
# with reportgenerator.

rm -rf TestResults/

# Execute the Xunit tests, sending output to both stdout and test.out
dotnet test --collect:"XPlat Code Coverage" | tee test.out

# Parse the {uuid}/coverage.cobertura.xml filename for passing to reportgenerator
cat test.out | grep coverage.cobertura.xml | awk '{$1=$1};1' > test_cobertura_filename.txt

cobertura_filename=`cat test_cobertura_filename.txt`
echo 'coverage filename: '$cobertura_filename

reportgenerator \
    "-reports:"$cobertura_filename \
    "-targetdir:coveragereport" \
    -reporttypes:Html
