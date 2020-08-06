TaskKeepAlive by pftq
Oct.2015
www.pftq.com/taskkeepalive/

The TaskKeepAlive script runs every 10 sec and checks that all exe files in the current folder, as well as any nested folders, are still running.  If not, it'll kill any hanging processes for those exe files and restart them.  Arguments can be passed to the TaskKeepAlive script to pass directly to the processes it restarts.  It is smart enough to wait a couple passes on a task to see if the task unfreezes before trying to restart.

The script is short and sweet.  The source code is included for anyone that might need to tweak it for their use.

The script is compiled for Windows 64-bit.