// ** React Import
import { ReactNode } from 'react'

// ** Next Import
import Link from 'next/link'

// ** MUI Imports
import IconButton from '@mui/material/IconButton'
import Box, { BoxProps } from '@mui/material/Box'
import { styled, useTheme } from '@mui/material/styles'
import Typography, { TypographyProps } from '@mui/material/Typography'

// ** Icons
import Close from 'mdi-material-ui/Close'

// ** Type Import
import { Settings } from 'src/@core/context/settingsContext'

// ** Configs
import themeConfig from 'src/configs/themeConfig'
import Logo from "src/@core/layouts/Logo";

interface Props {
  hidden: boolean
  navHover: boolean
  settings: Settings
  collapsedNavWidth: number
  menuLockedIcon?: ReactNode
  menuUnlockedIcon?: ReactNode
  navigationBorderWidth: number
  toggleNavVisibility: () => void
  saveSettings: (values: Settings) => void
  verticalNavMenuBranding?: (props?: any) => ReactNode
}

// ** Styled Components
const MenuHeaderWrapper = styled(Box)<BoxProps>(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  paddingRight: theme.spacing(4),
  justifyContent: 'space-between',
  transition: 'padding .25s ease-in-out',
  minHeight: theme.mixins.toolbar.minHeight
}))

const HeaderTitle = styled(Typography)<TypographyProps>({
  fontWeight: 700,
  lineHeight: 1.2,
  transition: 'opacity .25s ease-in-out, margin .25s ease-in-out'
})

const StyledLink = styled('a')({
  display: 'flex',
  alignItems: 'center',
  textDecoration: 'none'
})

const VerticalNavHeader = (props: Props) => {
  // ** Props
  const {
    hidden,
    navHover,
    settings,
    saveSettings,
    collapsedNavWidth,
    toggleNavVisibility,
    navigationBorderWidth,
    menuLockedIcon: userMenuLockedIcon,
    menuUnlockedIcon: userMenuUnlockedIcon,
    verticalNavMenuBranding: userVerticalNavMenuBranding
  } = props

  // ** Hooks & Vars
  const theme = useTheme()
  const { skin, direction, navCollapsed } = settings
  const menuCollapsedStyles = navCollapsed && !navHover ? { opacity: 0 } : { opacity: 1 }

  const svgFillSecondary = () => {
    if (skin === 'semi-dark' && theme.palette.mode === 'light') {
      return `rgba(${theme.palette.customColors.dark}, 0.68)`
    } else if (skin === 'semi-dark' && theme.palette.mode === 'dark') {
      return `rgba(${theme.palette.customColors.light}, 0.68)`
    } else {
      return theme.palette.text.secondary
    }
  }
  const svgFillDisabled = () => {
    if (skin === 'semi-dark' && theme.palette.mode === 'light') {
      return `rgba(${theme.palette.customColors.dark}, 0.38)`
    } else if (skin === 'semi-dark' && theme.palette.mode === 'dark') {
      return `rgba(${theme.palette.customColors.light}, 0.38)`
    } else {
      return theme.palette.text.disabled
    }
  }

  const menuHeaderPaddingLeft = () => {
    if (navCollapsed && !navHover) {
      if (userVerticalNavMenuBranding) {
        return 0
      } else {
        return (collapsedNavWidth - navigationBorderWidth - 40) / 8
      }
    } else {
      return 5.5
    }
  }

  const svgRotationDeg = () => {
    if (navCollapsed) {
      if (direction === 'rtl') {
        if (navHover) {
          return 0
        } else {
          return 180
        }
      } else {
        if (navHover) {
          return 180
        } else {
          return 0
        }
      }
    } else {
      if (direction === 'rtl') {
        return 180
      } else {
        return 0
      }
    }
  }

  return (
    <MenuHeaderWrapper className='nav-header' sx={{ pl: menuHeaderPaddingLeft() }}>
      {userVerticalNavMenuBranding ? (
        userVerticalNavMenuBranding(props)
      ) : (
        <Link href='/' passHref>
          <StyledLink>
            <Logo/>
            <HeaderTitle variant='h6' sx={{ ...menuCollapsedStyles, ...(navCollapsed && !navHover ? {} : { ml: 2 }) }}>
              {themeConfig.templateName}
            </HeaderTitle>
          </StyledLink>
        </Link>
      )}

      {hidden ? (
        <IconButton
          disableRipple
          disableFocusRipple
          onClick={toggleNavVisibility}
          sx={{ p: 0, backgroundColor: 'transparent !important' }}
        >
          <Close fontSize='small' />
        </IconButton>
      ) : (
        <IconButton
          disableRipple
          disableFocusRipple
          onClick={() => saveSettings({ ...settings, navCollapsed: !navCollapsed })}
          sx={{ p: 0, color: 'text.primary', backgroundColor: 'transparent !important' }}
        >
          {userMenuLockedIcon && userMenuUnlockedIcon ? (
            navCollapsed ? (
              userMenuUnlockedIcon
            ) : (
              userMenuLockedIcon
            )
          ) : (
            <Box
              width={22}
              fill='none'
              height={22}
              component='svg'
              viewBox='0 0 22 22'
              xmlns='http://www.w3.org/2000/svg'
              sx={{
                transform: `rotate(${svgRotationDeg()}deg)`,
                transition: 'transform .25s ease-in-out .35s'
              }}
            >
              <path
                fill={svgFillSecondary()}
                d='M11.4854 4.88844C11.0082 4.41121 10.2344 4.41121 9.75716 4.88844L4.51029 10.1353C4.03299 10.6126 4.03299 11.3865 4.51029 11.8638L9.75716 17.1107C10.2344 17.5879 11.0082 17.5879 11.4854 17.1107C11.9626 16.6334 11.9626 15.8597 11.4854 15.3824L7.96674 11.8638C7.48943 11.3865 7.48943 10.6126 7.96674 10.1353L11.4854 6.61667C11.9626 6.13943 11.9626 5.36568 11.4854 4.88844Z'
              />
              <path
                fill={svgFillDisabled()}
                d='M15.8683 4.88844L10.6214 10.1353C10.1441 10.6126 10.1441 11.3865 10.6214 11.8638L15.8683 17.1107C16.3455 17.5879 17.1193 17.5879 17.5965 17.1107C18.0737 16.6334 18.0737 15.8597 17.5965 15.3824L14.0779 11.8638C13.6005 11.3865 13.6005 10.6126 14.0779 10.1353L17.5965 6.61667C18.0737 6.13943 18.0737 5.36568 17.5965 4.88844C17.1193 4.41121 16.3455 4.41121 15.8683 4.88844Z'
              />
            </Box>
          )}
        </IconButton>
      )}
    </MenuHeaderWrapper>
  )
}

export default VerticalNavHeader
