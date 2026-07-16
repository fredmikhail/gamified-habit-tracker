import { expect, test } from '@playwright/test'

test('shows the login form for an anonymous user', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByRole('textbox', { name: 'Email' })).toBeVisible()

  await expect(page.getByLabel('Password')).toBeVisible()

  await expect(
    page.getByRole('checkbox', {
      name: 'Keep me signed in',
    }),
  ).toBeVisible()
})
