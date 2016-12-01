echo 'travis_fold:start:download-stylecop'
echo 'Downloading Stylecop'
curl -Lo scc.zip "https://github.com/enckse/StyleCopCmd/releases/download/v1.3.6/StyleCopCmd-1.3.6.zip"
echo 'travis_fold:end:download-stylecop'

sudo mkdir /opt/stylecop

echo 'travis_fold:start:install-stylecop'
echo 'Installing Stylecop'
sudo unzip scc.zip -d /opt/stylecop
echo 'travis_fold:end:install-stylecop'
