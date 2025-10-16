# NullOps Project Architecture

## Purpose of this project

The idea for this project came when we faced a similar challenge in our workplace.
Manual QA was always behind developers due to how complex features were and the lack of parallel testing environments.
Only one complex feature could be tested at a time.

So I've started my research. I've found several solutions, but none of them were exactly what I was looking for.

Here's a breakdown of solutions I've found:

| Solution                                      | Why it didn't work out                                                                                     |
|-----------------------------------------------|------------------------------------------------------------------------------------------------------------|
| just a k8s cluster                            | Doesn't have UI and will require DevOps engineers to create each new environment.                          |
| OpenShift                                     | Bound by corporate licenses. Lacks automation features. Complex to set up and use.                         |
| Developer Portals                             | Either require a lot of coding or paid. Not true OSS fashion.                                              |
| Manual docker-compose deployment              | Requires a developer to SSH onto target machine and manage running environments.                           |
| Automatic docker-compose deployment via CI/CD | Does not provide flexibility, complex to set up, no UI to select service versions/view active deployments. |

