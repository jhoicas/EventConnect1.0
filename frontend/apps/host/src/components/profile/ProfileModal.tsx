"use client";

import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalCloseButton,
  VStack,
  Tabs,
  TabList,
  Tab,
  TabPanels,
  TabPanel,
  FormControl,
  FormLabel,
  Input,
  Button,
  Avatar,
  SimpleGrid,
  Box,
  Text,
  useToast,
  HStack,
  IconButton,
  Badge,
} from "@chakra-ui/react";
import { useState, useEffect } from "react";
import { Camera, User, Lock, Save } from "lucide-react";

interface ProfileModalProps {
  isOpen: boolean;
  onClose: () => void;
  user: any;
  onSave: (data: any) => void;
}

// Galería de avatares predeterminados
const getDefaultAvatars = (userName: string = 'User') => ({
  hombres: [
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Felix&backgroundColor=b6e3f4",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Aneka&backgroundColor=c0aede",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=John&backgroundColor=d1d4f9",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Charlie&backgroundColor=ffd5dc",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Max&backgroundColor=ffdfbf",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Sam&backgroundColor=c7dbd4",
  ],
  mujeres: [
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Sophie&backgroundColor=b6e3f4",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Emma&backgroundColor=c0aede",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Luna&backgroundColor=d1d4f9",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Mia&backgroundColor=ffd5dc",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Zoe&backgroundColor=ffdfbf",
    "https://api.dicebear.com/7.x/avataaars/svg?seed=Lily&backgroundColor=c7dbd4",
  ],
  animales: [
    "https://api.dicebear.com/7.x/bottts/svg?seed=Felix&backgroundColor=b6e3f4",
    "https://api.dicebear.com/7.x/bottts/svg?seed=Aneka&backgroundColor=c0aede",
    "https://api.dicebear.com/7.x/bottts/svg?seed=Whiskers&backgroundColor=d1d4f9",
    "https://api.dicebear.com/7.x/bottts/svg?seed=Fluffy&backgroundColor=ffd5dc",
    "https://api.dicebear.com/7.x/bottts/svg?seed=Buddy&backgroundColor=ffdfbf",
    "https://api.dicebear.com/7.x/bottts/svg?seed=Lucky&backgroundColor=c7dbd4",
  ],
  profesional: [
    `https://api.dicebear.com/7.x/initials/svg?seed=${userName}&backgroundColor=6366f1`,
    `https://api.dicebear.com/7.x/initials/svg?seed=${userName}&backgroundColor=8b5cf6`,
    `https://api.dicebear.com/7.x/initials/svg?seed=${userName}&backgroundColor=ec4899`,
    `https://api.dicebear.com/7.x/initials/svg?seed=${userName}&backgroundColor=f59e0b`,
    `https://api.dicebear.com/7.x/initials/svg?seed=${userName}&backgroundColor=10b981`,
    `https://api.dicebear.com/7.x/initials/svg?seed=${userName}&backgroundColor=06b6d4`,
  ],
});

export default function ProfileModal({ isOpen, onClose, user, onSave }: ProfileModalProps) {
  const toast = useToast();
  const DEFAULT_AVATARS = getDefaultAvatars(user?.nombre_Completo || 'User');
  const [selectedAvatar, setSelectedAvatar] = useState(user?.avatar_URL || DEFAULT_AVATARS.profesional[0]);
  const [customImage, setCustomImage] = useState("");
  const [formData, setFormData] = useState({
    nombre_Completo: user?.nombre_Completo || "",
    email: user?.email || "",
    telefono: user?.telefono || "",
  });
  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  // Sincronizar formData cuando el usuario cambia
  useEffect(() => {
    if (user) {
      setFormData({
        nombre_Completo: user.nombre_Completo || "",
        email: user.email || "",
        telefono: user.telefono || "",
      });
    }
  }, [user]);

  const handleSave = async () => {
    // Usar SIEMPRE los valores del usuario actual para campos que no se están editando
    // Solo se debe enviar Nombre_Completo y Telefono si fueron modificados en el formulario
    const nombre = formData.nombre_Completo?.trim() || user?.nombre_Completo;
    const telefono = formData.telefono?.trim() || user?.telefono;
    
    // Validaciones básicas
    if (!nombre) {
      toast({
        title: "Error",
        description: "El nombre es obligatorio",
        status: "error",
        duration: 3000,
      });
      return;
    }

    try {
      const token = localStorage.getItem('token');
      const avatarURL = customImage || selectedAvatar;
      
      const payload = {
        Nombre_Completo: nombre,
        Telefono: telefono || "",
        Avatar_URL: avatarURL,
      };
      
      console.log('=== DEBUG: Guardando perfil ===');
      console.log('User actual:', user);
      console.log('FormData:', formData);
      console.log('Payload a enviar:', payload);
      console.log('JSON stringificado:', JSON.stringify(payload));

      const response = await fetch(`http://localhost:5555/api/Usuario/${user?.id}/perfil`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        throw new Error('Error al actualizar el perfil');
      }

      const data = await response.json();
      console.log('Respuesta del servidor:', data);

      // Actualizar Redux con los nuevos datos
      onSave({
        nombre_Completo: nombre,
        telefono: telefono,
        email: user?.email || formData.email,
        avatar_URL: avatarURL,
      });

      toast({
        title: "Perfil actualizado",
        description: "Tus cambios se han guardado correctamente",
        status: "success",
        duration: 3000,
      });
      onClose();
    } catch (error) {
      console.error('Error al guardar perfil:', error);
      toast({
        title: "Error",
        description: "No se pudo actualizar el perfil",
        status: "error",
        duration: 3000,
      });
    }
  };

  const handlePasswordChange = async () => {
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      toast({
        title: "Error",
        description: "Las contraseñas no coinciden",
        status: "error",
        duration: 3000,
      });
      return;
    }

    if (passwordData.newPassword.length < 8) {
      toast({
        title: "Error",
        description: "La contraseña debe tener al menos 8 caracteres",
        status: "error",
        duration: 3000,
      });
      return;
    }

    if (!passwordData.currentPassword) {
      toast({
        title: "Error",
        description: "Debes ingresar tu contraseña actual",
        status: "error",
        duration: 3000,
      });
      return;
    }

    try {
      const token = localStorage.getItem('token');
      
      const response = await fetch(`http://localhost:5555/api/Usuario/${user?.id}/cambiar-password`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          CurrentPassword: passwordData.currentPassword,
          NewPassword: passwordData.newPassword,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Error al cambiar la contraseña');
      }

      toast({
        title: "Contraseña actualizada",
        description: "Tu contraseña se ha cambiado correctamente",
        status: "success",
        duration: 3000,
      });
      setPasswordData({ currentPassword: "", newPassword: "", confirmPassword: "" });
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "No se pudo cambiar la contraseña",
        status: "error",
        duration: 3000,
      });
    }
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setCustomImage(reader.result as string);
        setSelectedAvatar("");
      };
      reader.readAsDataURL(file);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="2xl">
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>Mi Perfil</ModalHeader>
        <ModalCloseButton />
        <ModalBody pb={6}>
          <Tabs colorScheme="blue">
            <TabList>
              <Tab>
                <HStack>
                  <User size={16} />
                  <Text>Información</Text>
                </HStack>
              </Tab>
              <Tab>
                <HStack>
                  <Camera size={16} />
                  <Text>Avatar</Text>
                </HStack>
              </Tab>
              <Tab>
                <HStack>
                  <Lock size={16} />
                  <Text>Seguridad</Text>
                </HStack>
              </Tab>
            </TabList>

            <TabPanels>
              {/* Tab de Información Personal */}
              <TabPanel>
                <VStack spacing={4} align="stretch">
                  <FormControl isRequired>
                    <FormLabel>Nombre Completo</FormLabel>
                    <Input
                      value={formData.nombre_Completo}
                      onChange={(e) =>
                        setFormData({ ...formData, nombre_Completo: e.target.value })
                      }
                      placeholder="Juan Pérez"
                    />
                  </FormControl>

                  <FormControl isRequired>
                    <FormLabel>Email</FormLabel>
                    <Input
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      placeholder="juan@ejemplo.com"
                    />
                  </FormControl>

                  <FormControl>
                    <FormLabel>Teléfono</FormLabel>
                    <Input
                      value={formData.telefono}
                      onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                      placeholder="+57 300 123 4567"
                    />
                  </FormControl>

                  <Button
                    leftIcon={<Save size={16} />}
                    colorScheme="blue"
                    onClick={handleSave}
                    mt={4}
                  >
                    Guardar Cambios
                  </Button>
                </VStack>
              </TabPanel>

              {/* Tab de Avatar */}
              <TabPanel>
                <VStack spacing={6} align="stretch">
                  {/* Avatar Actual */}
                  <Box textAlign="center">
                    <Avatar
                      src={customImage || selectedAvatar}
                      size="2xl"
                      mb={2}
                    />
                    <Badge colorScheme="green">Avatar Actual</Badge>
                  </Box>

                  {/* Subir Imagen Personalizada */}
                  <Box>
                    <FormLabel>Subir Foto Personalizada</FormLabel>
                    <Input
                      type="file"
                      accept="image/*"
                      onChange={handleImageUpload}
                      p={1}
                    />
                    <Text fontSize="xs" color="gray.500" mt={1}>
                      Formatos: JPG, PNG. Máx 2MB
                    </Text>
                  </Box>

                  {/* Avatares Profesionales */}
                  <Box>
                    <FormLabel>Iniciales (Estilo Gmail)</FormLabel>
                    <SimpleGrid columns={6} spacing={3}>
                      {DEFAULT_AVATARS.profesional.map((avatar, index) => (
                        <Avatar
                          key={index}
                          src={avatar}
                          cursor="pointer"
                          border={selectedAvatar === avatar ? "3px solid" : "none"}
                          borderColor="blue.500"
                          onClick={() => {
                            setSelectedAvatar(avatar);
                            setCustomImage("");
                          }}
                          _hover={{ transform: "scale(1.1)" }}
                          transition="all 0.2s"
                        />
                      ))}
                    </SimpleGrid>
                  </Box>

                  {/* Avatares Hombres */}
                  <Box>
                    <FormLabel>Avatares Masculinos</FormLabel>
                    <SimpleGrid columns={6} spacing={3}>
                      {DEFAULT_AVATARS.hombres.map((avatar, index) => (
                        <Avatar
                          key={index}
                          src={avatar}
                          cursor="pointer"
                          border={selectedAvatar === avatar ? "3px solid" : "none"}
                          borderColor="blue.500"
                          onClick={() => {
                            setSelectedAvatar(avatar);
                            setCustomImage("");
                          }}
                          _hover={{ transform: "scale(1.1)" }}
                          transition="all 0.2s"
                        />
                      ))}
                    </SimpleGrid>
                  </Box>

                  {/* Avatares Mujeres */}
                  <Box>
                    <FormLabel>Avatares Femeninos</FormLabel>
                    <SimpleGrid columns={6} spacing={3}>
                      {DEFAULT_AVATARS.mujeres.map((avatar, index) => (
                        <Avatar
                          key={index}
                          src={avatar}
                          cursor="pointer"
                          border={selectedAvatar === avatar ? "3px solid" : "none"}
                          borderColor="blue.500"
                          onClick={() => {
                            setSelectedAvatar(avatar);
                            setCustomImage("");
                          }}
                          _hover={{ transform: "scale(1.1)" }}
                          transition="all 0.2s"
                        />
                      ))}
                    </SimpleGrid>
                  </Box>

                  {/* Avatares Animales */}
                  <Box>
                    <FormLabel>Avatares Divertidos</FormLabel>
                    <SimpleGrid columns={6} spacing={3}>
                      {DEFAULT_AVATARS.animales.map((avatar, index) => (
                        <Avatar
                          key={index}
                          src={avatar}
                          cursor="pointer"
                          border={selectedAvatar === avatar ? "3px solid" : "none"}
                          borderColor="blue.500"
                          onClick={() => {
                            setSelectedAvatar(avatar);
                            setCustomImage("");
                          }}
                          _hover={{ transform: "scale(1.1)" }}
                          transition="all 0.2s"
                        />
                      ))}
                    </SimpleGrid>
                  </Box>

                  <Button
                    leftIcon={<Save size={16} />}
                    colorScheme="blue"
                    onClick={handleSave}
                  >
                    Guardar Avatar
                  </Button>
                </VStack>
              </TabPanel>

              {/* Tab de Seguridad */}
              <TabPanel>
                <VStack spacing={4} align="stretch">
                  <FormControl isRequired>
                    <FormLabel>Contraseña Actual</FormLabel>
                    <Input
                      type="password"
                      value={passwordData.currentPassword}
                      onChange={(e) =>
                        setPasswordData({ ...passwordData, currentPassword: e.target.value })
                      }
                      placeholder="••••••••"
                    />
                  </FormControl>

                  <FormControl isRequired>
                    <FormLabel>Nueva Contraseña</FormLabel>
                    <Input
                      type="password"
                      value={passwordData.newPassword}
                      onChange={(e) =>
                        setPasswordData({ ...passwordData, newPassword: e.target.value })
                      }
                      placeholder="••••••••"
                    />
                    <Text fontSize="xs" color="gray.500" mt={1}>
                      Mínimo 8 caracteres
                    </Text>
                  </FormControl>

                  <FormControl isRequired>
                    <FormLabel>Confirmar Nueva Contraseña</FormLabel>
                    <Input
                      type="password"
                      value={passwordData.confirmPassword}
                      onChange={(e) =>
                        setPasswordData({ ...passwordData, confirmPassword: e.target.value })
                      }
                      placeholder="••••••••"
                    />
                  </FormControl>

                  <Button
                    leftIcon={<Lock size={16} />}
                    colorScheme="blue"
                    onClick={handlePasswordChange}
                    mt={4}
                  >
                    Cambiar Contraseña
                  </Button>
                </VStack>
              </TabPanel>
            </TabPanels>
          </Tabs>
        </ModalBody>
      </ModalContent>
    </Modal>
  );
}
