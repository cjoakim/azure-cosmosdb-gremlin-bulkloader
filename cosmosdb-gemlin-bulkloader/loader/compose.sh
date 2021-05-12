#!/bin/bash

# Execute 'docker-compose xxx', where the value of xxx is provided by
# the first command-line arg, or $1.  This script mostly exits for 
# documentation, and as a shortcut for docker-compose.
#
# Usage:
# $ ./compose.sh up
# $ ./compose.sh ps
# $ ./compose.sh down
#
# Chris Joakim, Microsoft, May 2021

docker-compose $1
