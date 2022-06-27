Working config:

```
# Greengrass debug console
config.vm.network "forwarded_port", guest: 1441, host: 1441, protocol: "tcp"
# Greengrass debug console (websocket)
config.vm.network "forwarded_port", guest: 1442, host: 1442, protocol: "tcp"
```

HOST:
```
timmatt@a483e78ce15e ~ % nc -v localhost 1441
localhost [127.0.0.1] 1441 (cadis-1) open
asdf
```

GUEST:
```
vagrant@ubuntu-focal:~/greengrass$ netcat -l 1441
asdf
```

GUEST NETSTAT:
```
vagrant@ubuntu-focal:~/greengrass$ netstat -an | grep LISTEN | grep 1441
tcp        0      0 0.0.0.0:1441            0.0.0.0:*               LISTEN
```

-----

Non-working:

```
# Greengrass debug console
config.vm.network "forwarded_port", guest: 1441, host: 1441, protocol: "tcp"
# Greengrass debug console (websocket)
config.vm.network "forwarded_port", guest: 1442, host: 1442, protocol: "tcp"
```

HOST (disconnects immediately):
```
timmatt@a483e78ce15e ~ % nc -v localhost 1441
localhost [127.0.0.1] 1441 (cadis-1) open
```

GUEST:
```
vagrant@ubuntu-focal:~/greengrass$ netcat -l 127.0.0.1 1441
```

GUEST NETSTAT:
```
vagrant@ubuntu-focal:~/greengrass$ netstat -an | grep LISTEN | grep 1441
tcp        0      0 127.0.0.1:1441          0.0.0.0:*               LISTEN
```

-----

Failure example:

```
timmatt@a483e78ce15e host % vagrant destroy && vagrant up && vagrant ssh
default: Are you sure you want to destroy the 'default' VM? [y/N] y
==> default: Forcing shutdown of VM...
==> default: Destroying VM and associated drives...
Bringing machine 'default' up with 'virtualbox' provider...
==> default: Importing base box 'ubuntu/focal64'...
==> default: Matching MAC address for NAT networking...
==> default: Checking if box 'ubuntu/focal64' version '20220419.0.0' is up to date...
==> default: Setting the name of the VM: host_default_1650658431833_40802
==> default: Clearing any previously set network interfaces...
==> default: Preparing network interfaces based on configuration...
default: Adapter 1: nat
==> default: Forwarding ports...
default: 1441 (guest) => 1441 (host) (adapter 1)
default: 1442 (guest) => 1442 (host) (adapter 1)
default: 22 (guest) => 2222 (host) (adapter 1)
==> default: Running 'pre-boot' VM customizations...
==> default: Booting VM...
==> default: Waiting for machine to boot. This may take a few minutes...
default: SSH address: 127.0.0.1:2222
default: SSH username: vagrant
default: SSH auth method: private key
default: Warning: Connection reset. Retrying...
default: Warning: Remote connection disconnect. Retrying...
default:
default: Vagrant insecure key detected. Vagrant will automatically replace
default: this with a newly generated keypair for better security.
default:
default: Inserting generated public key within guest...
default: Removing insecure key from the guest if it's present...
default: Key inserted! Disconnecting and reconnecting using new SSH key...
==> default: Machine booted and ready!
==> default: Checking for guest additions in VM...
==> default: Mounting shared folders...
default: /home/vagrant/.aws => /Users/timmatt/.aws
default: /home/vagrant/shared => /Users/timmatt/synced/code/greeneyes/blog-posts/001/shared
==> default: Detected mount owner ID within mount options. (uid: 1000 guestpath: /home/vagrant/.aws)
==> default: Detected mount group ID within mount options. (gid: 1000 guestpath: /home/vagrant/.aws)
==> default: Running provisioner: shell...
default: Running: inline script
default: E: Type 'ubuntu' is not known on line 50 in source list /etc/apt/sources.list
default: E: The list of sources could not be read.
default: Reading package lists...
default: Building dependency tree...
default: Reading state information...
default: E: Unable to locate package v4l-utils
default: E: Unable to locate package openjdk-17-jre-headless
==> default: Running provisioner: shell...
default: Running: inline script
default:   % Total    % Received % Xferd  Average Speed   Time    Time     Time  Current
default:                                  Dload  Upload   Total   Spent    Left  Speed
100 44.7M  100 44.7M    0     0  33.5M      0  0:00:01  0:00:01 --:--:-- 33.5M
default: /tmp/vagrant-shell: line 2: unzip: command not found
default: /tmp/vagrant-shell: line 3: /tmp/aws/install: No such file or directory
==> default: Running provisioner: file...
default: ../../../.git => $HOME/greeneyes/.git
==> default: Running provisioner: shell...
default: Running: inline script
default: HEAD is now at 1de765f TEMP fix incorrect Greengrass root directory, add checks to be extra safe
default: /home/vagrant
==> default: Running provisioner: shell...
default: Running: inline script
Welcome to Ubuntu 20.04.4 LTS (GNU/Linux 5.4.0-109-generic x86_64)

* Documentation:  https://help.ubuntu.com
* Management:     https://landscape.canonical.com
* Support:        https://ubuntu.com/advantage

System information as of Fri Apr 22 20:15:15 UTC 2022

System load:  0.29              Processes:               123
Usage of /:   3.5% of 38.71GB   Users logged in:         0
Memory usage: 19%               IPv4 address for enp0s3: 10.0.2.15
Swap usage:   0%


1 update can be applied immediately.
To see these additional updates run: apt list --upgradable


vagrant@ubuntu-focal:~$
```
