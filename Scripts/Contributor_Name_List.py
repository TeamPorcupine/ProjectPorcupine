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


url = 'https://api.github.com/repos/TeamPorcupine/ProjectPorcupine/contributors'
page=1
names=[]
while True:
	output=get_jsonparsed_data(url+"?page=%i"%page)
	if len(output)==0:
		break;

	for row in output:
		temp=row["login"]
		names+=[temp[0].upper()+temp[1:]]
	page+=1
	
names.sort()
for name in names:
	print (name)
