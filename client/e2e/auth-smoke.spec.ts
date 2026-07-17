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

test('registers, restores the session, logs out, and logs back in', async ({
  page,
}) => {
  const uniqueId = Date.now().toString()

  const email = `e2e_${uniqueId}@example.com`
  const username = `e2e_${uniqueId}`
  const password = 'E2eTestPassword123!'
  const timeZone = 'America/Toronto'

  await page.goto('/')

  await page
    .getByRole('button', {
      name: 'Register',
      exact: true,
    })
    .click()

  const registerForm = page.locator('form')

  await registerForm
    .getByRole('textbox', {
      name: 'Email',
    })
    .fill(email)

  await registerForm
    .getByRole('textbox', {
      name: 'Username',
    })
    .fill(username)

  await registerForm.getByLabel('Password').fill(password)

  await registerForm
    .getByRole('textbox', {
      name: 'Time zone',
    })
    .fill(timeZone)

  await registerForm
    .getByRole('button', {
      name: 'Create account',
    })
    .click()

  await expect(page.getByText(`Signed in as ${username}`)).toBeVisible()

  await page.reload()

  await expect(page.getByText(`Signed in as ${username}`)).toBeVisible()

  await page
    .getByRole('button', {
      name: 'Sign out',
    })
    .click()

  await expect(
    page.getByRole('button', {
      name: 'Register',
      exact: true,
    }),
  ).toBeVisible()

  await page
    .getByRole('button', {
      name: 'Sign in',
      exact: true,
    })
    .first()
    .click()

  const loginForm = page.locator('form')

  await loginForm
    .getByRole('textbox', {
      name: 'Email',
    })
    .fill(email)

  await loginForm.getByLabel('Password').fill(password)

  await loginForm
    .getByRole('button', {
      name: 'Sign in',
      exact: true,
    })
    .click()

  await expect(page.getByText(`Signed in as ${username}`)).toBeVisible()
})
