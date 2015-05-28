How to merge new upstream changes:

1. Checkout upstream branch and pull latest changes.

   $ git checkout mono-4.2.0-pre-branch
   $ git pull
   $ git submodule update --init --recursive

2. Create temp merge branch

   $ git checkout -b temp-merge
   
3. Use `git log` to find last revision that was merged into `work-newtls`.

   You will find this in the commit logs.
   
   At the moment, that's revision `5493b0a73be217b143390f31d7142057ec4c9b01`.
   
4. Use `git rebase <LAST-UPSTREAM> --onto work-newtls`.

   $ git rebase 5493b0a73be217b143390f31d7142057ec4c9b01 --onto work-newtls
   
5. Switch back to `work-newtls`.

   $ git checkout work-newtls
   $ git submodule update --init --recursive
   
6. Squash-merge the `temp-merge` branch.

   $ git merge --squash --no-commit temp-merge
   
7. Commit and include the upstream revision in the log message.

   $ git commit -m "Merge mono-4.2.0-pre-branch commit 6b901f4a7ddfa35ea5fae43b6a03a32bef19e15b."
   
8. Delete the temp merge branch

   $ git branch -D temp-merge
   

