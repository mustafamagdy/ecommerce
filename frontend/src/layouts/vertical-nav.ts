// ** Icon imports
import HomeOutline from 'mdi-material-ui/HomeOutline'
import EmailOutline from 'mdi-material-ui/EmailOutline'
import ShieldOutline from 'mdi-material-ui/ShieldOutline'

// ** Type import
import {VerticalNavItemsType} from 'src/@core/layouts/types'

//icon: HomeOutline,
const navigation = (): VerticalNavItemsType => {
  return [
    {
      title: 'Dashboard',
      path: '/home',
      icon: HomeOutline,
    },
    {
      title: 'Tenants',
      icon: HomeOutline,
      children: [
        {
          title: 'All Tenants',
          path: '/tenants',
          icon: HomeOutline
        },
        {
          title: 'History',
          path: '/history',
          icon: HomeOutline
        },
        {
          title: 'Payments',
          path: '/invoice/list',
          icon: HomeOutline
        },
        {
          title: 'Support',
          path: '/support',
          icon: HomeOutline
        },
        {
          title: 'Packages',
          path: '/packages',
          icon: HomeOutline
        }
      ]
    },
    {
      title: 'System',
      path: '/system',
      icon: HomeOutline,
      children: [
        {
          title: 'Users',
          path: '/user/list',
          icon: HomeOutline
        },
        {
          title: 'Roles',
          path: '/roles',
          icon: HomeOutline
        },
        {
          title: 'Maintenance',
          path: '/maintenance',
          icon: HomeOutline
        },
        {
          title: 'Workflows',
          path: '/workflows',
          icon: HomeOutline
        },
        {
          title: 'Settings',
          path: '/settings',
          icon: HomeOutline
        },
        {
          title: 'Monitoring',
          path: '/monitoring',
          icon: HomeOutline
        },
        {
          title: 'Updates',
          path: '/updates',
          icon: HomeOutline
        }
      ]
    }
  ]
}

export default navigation
