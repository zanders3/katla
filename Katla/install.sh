#!/bin/sh
# prepares a ubuntu 12.04 server instance to act as a Katla server
# run as sudo ./install.sh

# configure the vanilla container with the required software
apt-get install lxc mono-complete nginx

# append the lxc dns nameserver (dnsmasq on 10.0.3.1) which will resolve server hostnames
echo "nameserver 10.0.3.1" >> /etc/resolvconf/resolv.conf.d/head
resolvconf -u
