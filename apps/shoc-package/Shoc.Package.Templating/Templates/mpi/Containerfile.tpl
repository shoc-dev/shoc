# Use the base image "mpi" with a configurable tag (default to latest)
FROM ghcr.io/shoc-dev/containers/library/{{ implementation ?? "openmpi"}}-builder-ubuntu:{{ tag ?? "latest" }}

# Define environment variables for user and group IDs with a default value for uid and user
ENV SHOC_UID={{ uid ?? system.uid }}
ENV SHOC_USER={{ user ?? system.user }}

# Create a non-root user with a specific UID and GID
RUN id -u ubuntu   >/dev/null 2>&1 && userdel ubuntu || echo "user ubuntu not present" && \
    getent group ubuntu >/dev/null 2>&1 && groupdel ubuntu || echo "group ubuntu not present"

RUN mkdir -p /home/$SHOC_USER && chmod 755 /home/$SHOC_USER

RUN addgroup \
        --gid=$SHOC_UID \
        $SHOC_USER 
        
RUN adduser \
        --uid=$SHOC_UID \
        --ingroup=$SHOC_USER \
        --disabled-password \
        $SHOC_USER

RUN chown -R $SHOC_USER:$SHOC_USER /home/$SHOC_USER

# Set the working directory and ensure it has the right permissions
WORKDIR /app
RUN chown -R $SHOC_USER:$SHOC_USER /app

# Ensure entrypoint is run from the shoc user
USER $SHOC_UID

# Copy .sshd_config file
RUN cp /etc/templates/.sshd_config /home/$SHOC_USER/.sshd_config

# Replace username placeholder from sshd configuration
RUN sed -i -e "s/{SHOC_USER_NAME}/$SHOC_USER/g" /home/$SHOC_USER/.sshd_config

# Copy entrypoint file
RUN cp /etc/templates/entrypoint.sh /app/entrypoint.sh

# Set the working directory and ensure it has the right permissions
RUN chown -R $SHOC_USER:$SHOC_USER /home/$SHOC_USER

# Set proper working directory
WORKDIR /home/$SHOC_USER

# Copy all files into the ~/ directory
COPY . /home/$SHOC_USER

{{if make}}
RUN make {{ make }}
{{end}}

# Set the entrypoint for the container to be predefined script
ENTRYPOINT ["/app/entrypoint.sh"]
