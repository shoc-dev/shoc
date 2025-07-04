#!/usr/bin/env bash

set_intel_vars=/opt/intel/oneapi/setvars.sh
if [ -f $set_intel_vars ]; then
  source $set_intel_vars
fi

function resolve_host() {
  host="$1"
  check="nslookup $host"
  max_retry=10
  counter=0
  backoff=0.1
  until $check > /dev/null
  do
    if [ $counter -eq $max_retry ]; then
      echo "Couldn't resolve $host"
      return
    fi
    sleep $backoff
    echo "Couldn't resolve $host... Retrying"
    ((counter++))
    backoff=$(echo - | awk "{print $backoff + $backoff}")
  done
  echo "Resolved $host"
}

if [ "$K_MPI_JOB_ROLE" == "launcher" ]; then

  while read -r line; do
    # Extract the hostname from either MPICH or OpenMPI-style hostfile
    host=$(echo "$line" | cut -d ':' -f 1 | awk '{print $1}')
    [ -n "$host" ] && resolve_host "$host"
  done < /etc/mpi/hostfile
fi

exec "$@"