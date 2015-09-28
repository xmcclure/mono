Currently tracking mono-4.2.0-branch commit e325236fa31f60890a1ba701ccfdae7bf74509b6.

Last merged on Sep 28th in our commit 96271d7aa9a06ed1406fdf6e97a02e152ae17fa6.

How to merge new upstream changes:

1. Checkout upstream branch and pull latest changes.

   $ git checkout mono-4.2.0-branch
   $ git pull
   $ git submodule update --init --recursive

2. Create temp merge branch

   $ git checkout -b temp-merge
   
3. Use `git log` to find last revision that was merged into `work-newtls`.

   You will find this in the commit logs.
   
   At the moment, that's revision `e325236fa31f60890a1ba701ccfdae7bf74509b6`.
   
4. Use `git rebase <LAST-UPSTREAM> --onto work-newtls`.

   $ git rebase e325236fa31f60890a1ba701ccfdae7bf74509b6 --onto work-newtls
   
5. Switch back to `work-newtls`.

   $ git checkout work-newtls
   $ git submodule update --init --recursive
   
6. Squash-merge the `temp-merge` branch.

   $ git merge --squash --no-commit temp-merge
   
7. Commit and include the upstream revision in the log message.

   $ git commit -m "Merge mono-4.2.0-pre-branch commit e325236fa31f60890a1ba701ccfdae7bf74509b6."
   
8. Delete the temp merge branch

   $ git branch -D temp-merge
   

