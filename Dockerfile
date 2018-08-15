# Self build docker file
FROM gavinjonespf/docker-toolbox:latest
COPY * /home/jenkins/Cake.Recipe/
RUN pwsh -c "& cd /home/jenkins/Cake.Recipe/; ./build.ps1 -target build"

# Test self build mounts:
# docker run --rm -v ${PWD}:/data alpine ls /data
# Manually test build
# docker run --rm -it -v ${PWD}:/mnt/build gavinjonespf/docker-toolbox:latest pwsh