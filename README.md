[![Build Status](https://api.travis-ci.org/arxes-tolina/Tolina.StartAs.svg)](https://travis-ci.org/arxes-tolina/Tolina.StartAs)

## Tolina.StartAs
This software allows you (caller) to start a new process as a different user (target user) with following constraints:
- the target user's profile is loaded
- the target user's permissions are used
- all caller's environment variables are inherited, except APPDATA, LOCALAPPDATA, HOMEDRIVE, HOMEPATH, USERDOMAIN, USERNAME, USERPROFILE which are replaced with the target user's
- the target user's stdout and stderr are redirected to the caller's stdout and stderr
- no new console / window is opened

## Command line options
| Name             | Description                            |
|:-----------------|:---------------------------------------|
| -u, --user       | Useraccount to run under               |
| -p, --password   | Password of the useraccount            |
| -e, --executable | Path to executable                     |
| -a, --arguments  | Additional arguments (optional)        |
| -w, --workdir    | Working directory                      |
| -v, --verbose    | Prints all messages to standard output.|

