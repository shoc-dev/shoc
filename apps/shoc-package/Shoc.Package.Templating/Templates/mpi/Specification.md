### Specification.md

This section outlines the expected properties and configuration options for the MPI template. These properties can be defined in your `build.shoc.yaml` file to customize the generated container image for MPI applications.

#### Properties

- **tag** (`string`, optional):
    - The tag for the base image. Defaults to `latest`.
    - Example: `"custom-version"`

- **uid** (`int`, optional):
    - The user ID to create within the container. Defaults to `1000`.
    - Example: `1000`

- **user** (`string`, optional):
    - The user name to create within the container. Defaults to `"shoc"`.
    - Example: `"shoc"`

- **make** (`string`, optional):
    - A make goal to execute. Used to build your executable from source.
    - Example: `"all"`
  
- **entrypoint** (`array`, required):
    - An array of entrypoint commands. Required to ensure safe container execution. Defaults to `[]`.
    - Example: `["./pi"]`

---

#### Notes

1. The **tag** property allows you to specify a particular MPI version or a base image with MPI pre-installed.
2. The **uid** and **user** properties enhance security by allowing non-root execution within the container.
3. The **entrypoint** is a mandatory property defining how your MPI application starts.
4. The **make** property can be optionally provided to build from sources.