# Contributing

Thanks for helping improve SolidFlow.

## Development workflow

1. Create a branch from `main`.
2. Keep the change focused and include tests for behavior changes.
3. Run the local quality gates:

   ```bash
   dotnet restore
   dotnet build --configuration Release
   dotnet test --configuration Release --no-build
   ```

4. Open a pull request that explains the problem, the design choice, and how
   the change was verified.

## Design guidelines

- Keep domain behavior in `SolidFlow.Core`; keep transport concerns in the API.
- Prefer focused interfaces over broad service contracts.
- Add behavior through new policies when the existing contract is sufficient.
- Avoid framework dependencies in the core project.
- Treat public contracts as deliberate API decisions.

By contributing, you agree that your contribution is licensed under the
project's MIT License.
