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


url = 'https://api.github.com/repos/TeamPorcupine/ProjectPorcupine/stats/contributors'
output=get_jsonparsed_data(url)

names=[]
for row in output:
	names+=[row["author"]["login"].capitalize()]

names.sort()
for name in names:
	print (name)