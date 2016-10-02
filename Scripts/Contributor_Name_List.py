#!/usr/bin/env python

try:
    # For Python 3.0 and later
    from urllib.request import urlopen
except ImportError:
    # Fall back to Python 2's urllib2
    from urllib2 import urlopen

import json


def get_jsonparsed_data(url):
    """
    Receive the content of ``url``, parse it as JSON and return the object.

    Parameters
    ----------
    url : str

    Returns
    -------
    dict
    """
    response = urlopen(url)
    data = response.read().decode("utf-8")
    return json.loads(data)

names=[]

#Main PP repository
url = 'https://api.github.com/repos/TeamPorcupine/ProjectPorcupine/contributors'
page=1
while True:
	output=get_jsonparsed_data("%s?page=%i"%(url,page))
	if len(output)==0:
		break;

	for row in output:
		temp=row["login"]
		names+=[temp[0].upper()+temp[1:]]
	page+=1

#This is for the localization database. As it isn't in the master branch, we have to do some trickery.
page=1
url = 'https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits?sha=Someone_will_come_up_with_a_proper_naming_scheme_later'
while True:
	output=get_jsonparsed_data("%s&page=%i"%(url,page))
	if len(output)==0:
		break;

	for row in output:
		temp=row["committer"]["login"]
		names+=[temp[0].upper()+temp[1:]]
	page+=1

names=list(set(names))	#This will ensure that only unique values are included
names.sort()
for name in names:
	print (name)
