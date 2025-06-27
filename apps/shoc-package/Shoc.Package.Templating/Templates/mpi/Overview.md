### Overview.md

The **MPI (Message Passing Interface) Template** is designed for users needing to run parallel applications that utilize the MPI standard for inter-process communication.This template provides a robust foundation for building high-performance computing (HPC) containers, enabling efficient execution of distributed workloads across multiple nodes or processes. It leverages established MPI implementations to ensure compatibility and performance for your parallel codes.

#### Key Features
- **Parallel Execution**: Supports distributed computing using MPI for scalable applications.
- **Configurable Processes/Nodes**: Easily define the number of MPI processes and compute nodes.
- **Customizable Arguments**: Pass specific command-line arguments to your MPI application.
- **Non-root Execution**: The container runs under a non-root user for enhanced security.
- **Custom Entrypoint**: Define a custom entrypoint for the container to specify how your MPI application starts.