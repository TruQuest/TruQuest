#!/bin/bash

set -e
cd /singlefile/output
mkdir $1
npx single-file --browser-executable-path=/usr/bin/google-chrome --output-directory=/singlefile/output/$1 --browser-args="[\"--no-sandbox\", \"--disable-extensions-except=/singlefile/ublock\"]" --browser-headless=false --urls-file /singlefile/input/$1.txt --filename-template="{url-href-flat}.html"