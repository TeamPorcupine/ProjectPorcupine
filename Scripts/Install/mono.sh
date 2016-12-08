#! /bin/sh

# Get Mono Version from github repo
echo 'travis_fold:start:get-version'
echo 'Retrieving mono version'
curl -o "mono-version.txt" "https://raw.githubusercontent.com/enckse/travis-mono-scripts/master/mono-version.txt"
VER=$(cat "mono-version.txt")
echo 'travis_fold:end:get-version'

# Download from mono-project.com
echo 'travis_fold:start:download-mono'
echo 'Downloading Mono'
curl -o mono.pkg "http://download.mono-project.com/archive/$VER/macos-10-x86/MonoFramework-MDK-$VER.macos10.xamarin.x86.pkg"
echo 'travis_fold:end:download-mono'

# Install mono
echo 'travis_fold:start:install-mono'
echo 'Installing mono v'$VER 
sudo installer -pkg mono.pkg -target /
echo 'travis_fold:end:install-mono'
