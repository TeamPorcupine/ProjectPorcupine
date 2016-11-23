wget -O "mono-version.txt" "https://raw.githubusercontent.com/enckse/travis-mono-scripts/master/mono-version.txt"
VER=$(cat "mono-version.txt")
wget "http://download.mono-project.com/archive/$VER/macos-10-x86/MonoFramework-MDK-$VER.macos10.xamarin.x86.pkg"
sudo installer -pkg "MonoFramework-MDK-$VER.macos10.xamarin.x86.pkg" -target /