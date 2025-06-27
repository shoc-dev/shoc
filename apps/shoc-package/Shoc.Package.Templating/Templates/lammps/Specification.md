### Specification.md

This section outlines the expected properties and configuration options for the LAMMPS template. These properties can be defined in your `build.shoc.yaml` file to customize the generated container image.

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

- **entrypoint** (`array`, required):
    - An array of entrypoint commands.
    - Example: `["/usr/local/bin/lmp", "-in", "in.lammps"]`

---

#### Notes

1. The **tag** property allows you to specify a specific version of the LAMMPS base image.
2. The **uid** and **user** properties enhance security by allowing non-root execution.
3. The **entrypoint** is a critical, required property that dictates the primary command executed upon container startup.