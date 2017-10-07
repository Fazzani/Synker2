#!/usr/bin/env bash

sudo nano /etc/systemd/system/synkerwebclient.service
sudo systemctl disable synkerwebclient.service
sudo systemctl enable synkerwebclient.service
sudo service synkerwebclient start
sudo service synkerwebclient status