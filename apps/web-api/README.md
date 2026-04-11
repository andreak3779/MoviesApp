# MoviesAPI
Example web API using REST for managing my movie collection.

---

## Running Tests

To run the test suite, use the following command in the project root:

```
dotnet test
```

---

## API Endpoints

The API exposes the following endpoints for managing movies:

- `GET /api/movies` - Get all movies.
- `GET /api/movies/{id}` - Get a movie by ID.
- `POST /api/movies` - Create a new movie.
- `PUT /api/movies/{id}` - Update an existing movie.
- `DELETE /api/movies/{id}` - Delete a movie.

### Example Movie Object

```json
{
  "id": 1,
  "title": "Inception",
  "genre": "Sci-Fi",
  "releaseYear": 2010
}
```

---

## Workflow for Movies API

This document describes the workflow branching model for managing the development of the Movies API project.

### Branches

#### Main Branches
- **`main`**: The production-ready branch. Always reflects the latest stable release.
- **`develop`**: The integration branch for new features. Contains the latest development changes.

#### Supporting Branches
- **Feature Branches**: Used to develop new features. Merged into `develop`.
  - Naming convention: `feature/<feature-name>`
- **Release Branches**: Used to prepare for a new production release. Merged into both `main` and `develop`.
  - Naming convention: `release/<version-number>`
- **Hotfix Branches**: Used to fix critical issues in production. Merged into both `main` and `develop`.
  - Naming convention: `hotfix/<version-number>`

---

## Workflow

### 1. Start a New Feature
1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/<feature-name>
   ```
2. Develop the feature and commit changes.

3. Push the branch to the remote repository:
   ```bash
   git push -u origin feature/<feature-name>
   ```

4. When the feature is complete, create a pull request to merge into `develop`.

---

### 2. Start a Release
1. Create a release branch from `develop`:
   ```bash
   git checkout develop
   git pull
   git checkout -b release/<version-number>
   ```

2. Perform final testing, bug fixes, and version updates.

3. Merge the release branch into `main`:
   ```bash
   git checkout main
   git pull
   git merge --no-ff release/<version-number>
   git tag -a v<version-number> -m "Release <version-number>"
   git push origin main --tags
   ```

4. Merge the release branch back into `develop`:
   ```bash
   git checkout develop
   git pull
   git merge --no-ff release/<version-number>
   git push origin develop
   ```

5. Delete the release branch:
   ```bash
   git branch -d release/<version-number>
   git push origin --delete release/<version-number>
   ```

---

### 3. Hotfix Workflow
1. Create a hotfix branch from `main`:
   ```bash
   git checkout main
   git pull
   git checkout -b hotfix/<version-number>
   ```

2. Fix the issue and commit changes.

3. Merge the hotfix branch into `main`:
   ```bash
   git checkout main
   git pull
   git merge --no-ff hotfix/<version-number>
   git tag -a v<version-number> -m "Hotfix <version-number>"
   git push origin main --tags
   ```

4. Merge the hotfix branch back into `develop`:
   ```bash
   git checkout develop
   git pull
   git merge --no-ff hotfix/<version-number>
   git push origin develop
   ```

5. Delete the hotfix branch:
   ```bash
   git branch -d hotfix/<version-number>
   git push origin --delete hotfix/<version-number>
   ```

---

## Best Practices
- Keep commits small and focused.
- Write meaningful commit messages.
- Regularly pull changes from `develop` to avoid merge conflicts.
- Ensure all tests pass before merging into `develop` or `main`.

---

## Example Commands

### Create a Feature Branch
```bash
git checkout -b feature/add-movie-endpoint
```

### Merge a Feature Branch
```bash
git checkout develop
git merge --no-ff feature/add-movie-endpoint
git push origin develop
```

### Start a Release
```bash
git checkout -b release/1.0.0
```

### Apply a Hotfix
```bash
git checkout -b hotfix/1.0.1
```

---

## References
- [GitFlow Workflow](https://nvie.com/posts/a-successful-git-branching-model/)
- [Git Documentation](https://git-scm.com/doc)
