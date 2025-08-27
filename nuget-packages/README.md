# Release Process of packages by FHI

This document details the release of software packages and software package conventions used by FHI. <br>
Use this guide to verify your package release follows FHI convention.<br>
Packages released across multiple software package hosting sites (such as NuGet, npm and Github) should follow the same FHI ruleset.

### Principles of a good package release

1. Clear; The package and its intended use is clear from name, description and documentation. The changes in an updated package are clear from the version number.
2. Verifyable; The package is verifyable and identifable as an FHI package. 
3. Documented; The package is sufficiently documented in a way that makes changes traceable for the users.
4. Purposeful; The package is released and updated with purpose and consideration, which means it does not create unnecessary work for the users of the package.

## Naming Convention

<b>Packages released by FHI can be identified through these naming conventions:</b>
* NuGet-packages should have ID prefixes with the name "`TBD`". This prefix ID is reserved for FHI and all NuGet-packages released by FHI should have this prefix.
* For packages that are not released as NuGet-packages:
    * Use a naming ID similar to that of a NuGet-package.
    * Example: npm packages should use the namespace @`TBD`/ for packages released by FHI.
    * See PACKAGE MANAGERS USED BY FHI for full list of registered namespaces.
* Name of package should always follow the prefix ID. Ensure this name clearly conveys its use-case.
* After the name of the package there should optionally, but more often than not, follow the type of package released (example: CLI, logger, API, authenticator, container).

<b>Examples of NuGet-package names following FHI convention:</b>
* `TBD`.HelseIdSelvbetjening.CLI
* `TBD`.Authorization.Extensions

## Versioning

FHI follows the <a href="https://semver.org/">Semantic Versioning 2.0.0 Standard</a> for release of packages, as is required by multiple software package managers (such as NuGet or npm). <br>
Semantic versioning uses the format X.Y.Z where X, Y, Z represents a range of positive numbers starting at the number 0.

<b>MAJOR: Breaking changes (Versions X.y.z)</b>
* Changes which are incompatible with previous versions, i.e changes where the users has to change their configuration or usage to be compatible with the new package release, should always be followed by an increase in MAJOR version (X.y.z).
* Even seemingly small changes, such as changing the name of an API-endpoint, requires an update to the MAJOR version. Therefore;
    * Avoid releasing breaking changes often and consider collecting multiple breaking changes into one release when possible.
    * Calculate the gain against the cost of doing smaller breaking changes.
    * Avoid releasing the first MAJOR version of a package, version 1.0.0, until package has properly left the early stages of development.
* Quickly release a MAJOR version when a breaking change is accidentally released in a MINOR or PATCH version, even if code has not been updated between releases.
* It might seem sensible to update a MAJOR version after a conceptually important release, but this breaks Semantic Versioning standard. 
    * Do not update the MAJOR version unless breaking changes are introduced.
    * Avoid what is called "Romantic versioning". 

<b>MINOR: New features (Versions x.Y.z)</b>
* New fetures and functionality which are compatible with previous releases in the same MAJOR release (X.y.z) should update the MINOR version of the package (x.Y.z).
* Marking new features correctly as MINOR versions (x.Y.z) signals that users may choose to update their version of the installed package without breaking their integration.
* Releasing a new MAJOR version (X.y.z) resets both the MINOR (x.Y.z) and PATCH (x.y.Z) version to 0.

<b>PATCH: Bug fixes or backwards-compatible changes (Versions x.y.Z)</b>
* Bug fixes and backward compatible changes within the same MINOR release (i.e x.Y.z) should update the PATCH release of the package (x.y.Z).
* Releasing a new MINOR version (x.Y.z) resets the PATCH version (x.y.Z) to 0.

<b>PRERELEASE: Packages may use prerelease versioning such as --alpha.X or --beta.X to denote an unstable release:</b>
* A --alpa.X release is used for the earliest versions of the release, which contains untested features or changes to the package. It may also lack features or changes planned for the finished release.
* A --beta.X release is used following a --alpa.X release when all planned features or changes are implemented, but has yet to fix all known bugs or issues for the finished release.
* Packages should not leave prerelease versions until release is stable. This avoids unnecessary confusion and frequent updates for the users.

## Required by FHI

FHI packages should always have certain elements present. <br>
These elements helps users verify the validity of a package released by FHI and upholds FHI packages to an organization-wide standard of release. <br>

<b>Verify that these required elements are present on release of a package:</b>
* Follow the naming convention of FHI packages. See section NAMING CONVENTION.
* Follows proper Semantic Versioning standards. See section VERSIONING.
* Package metadata:
    * README detailing installation guide, usage notes and optionally a link to further documentation.
    * CONTRIBUTIONS which contain contributing developers of the package release.
    * FHI logo
    * Copyright (example: Copyright 2025 Folkehelseinstituttet (FHI))
    * Authors (example: Folkehelseinstituttet (FHI))
    * Package description
    * Package license for open source projects
    * Repository URL
* Release notes detailing the breaking changes, new features or bugfixes present in the releasing version.

## Package managers used by FHI

Consult maintainers found in "CONTACT" for registering package ID/ unique namespaces for FHI that do not exist currently.<br>
The package managers on this list has verifyable identification which makes it clear that its namespace is owned by FHI.<br>
Verifyable packages lessens security risks.

| Package Manager | Namespace/ Identification  | Verified/ Reserved |
|-----------------|----------------------------|--------------------|
|NuGet            |`TBD`                       |No                  |
|npm              |`TBD`                       |No                  |

## Contact

Todo