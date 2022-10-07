## Capture loop

This is a minimal component, written in Bash, that:

- Captures a frame from a webcam
- Checks to see if more than 10 frames have been captured. If so, it will delete the oldest frame until there are 10
  frames remaining.
- Sleeps for 1 second
- Goes back to the top of the loop
